using System.Globalization;
using ImageChartsLib;
using Microsoft.Extensions.Localization;
using PatientAnalytics.Models;
using PatientAnalytics.Utils.Localization;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PatientAnalytics.Services;

public class ReportService
{
    private readonly IStringLocalizer<ApiResponseLocalized> _localized;

    public ReportService(IStringLocalizer<ApiResponseLocalized> localized)
    {
        Settings.License = LicenseType.Community;
        _localized = localized;
    }

    public ReportResponse GenerateReportForPatient(Patient patient)
    {
        var temperatures = patient.Temperatures.Select(x => (x.DateCreated, x.TemperatureCelsius));
        var bloodPressures = patient.BloodPressures.Select(x =>
            (x.DateCreated, x.BloodPressureSystolic + "/" + x.BloodPressureDiastolic));
        var weights = patient.Weights.Select(x => (x.DateCreated, x.WeightKg)).OrderBy(x => x.DateCreated);
        var heights = patient.Heights.Select(x => (x.DateCreated, x.HeightCm)).OrderBy(x => x.DateCreated);

        double? firstBmi = null;
        DateTime? firstBmiDate = null;
        double? lastBmi = null;
        DateTime? lastBmiDate = null;

        if (weights.Count() > 0)
        {
            var firstMatchingPair = (from item1 in weights
                    join item2 in heights on item1.Item1.Date equals item2.Item1.Date 
                    select new Tuple<DateTime, Tuple<double, double>>(
                        item1.Item1, 
                        new Tuple<double, double>(item1.Item2, item2.Item2)))
                .FirstOrDefault();

            if (firstMatchingPair != null)
            {
                var weight = firstMatchingPair.Item2.Item1;
                var height = firstMatchingPair.Item2.Item2;
                firstBmiDate = firstMatchingPair.Item1;

                firstBmi = weight / (height / 100 * height / 100);
            }
        }

        
        if (weights.Count() > 1)
        {
            var lastMatchingPair = (from item1 in weights
                    join item2 in heights on item1.Item1.Date equals item2.Item1.Date
                    select new Tuple<DateTime, Tuple<double, double>>(
                        item1.Item1, 
                        new Tuple<double, double>(item1.Item2, item2.Item2)))
                .LastOrDefault();

            if (lastMatchingPair != null)
            {
                var weight = lastMatchingPair.Item2.Item1;
                var height = lastMatchingPair.Item2.Item2;
                lastBmiDate = lastMatchingPair.Item1;
                
                lastBmi = weight / (height / 100 * height / 100);
            }
        }

        byte[]? temperatureBarChart = null;
        if (temperatures.Count() > 0)
        {
            temperatureBarChart = GenerateBarChart(temperatures, _localized["ReportTitles_Temperatures"]);
        }
        
        byte[]? weightLineChart = null;
        if (weights.Count() > 0)
        {
            weightLineChart= GenerateLineChart(weights, _localized["ReportTitles_Weight"]);
        }
        
        byte[]? heightLineChart = null;
        if (heights.Count() > 0)
        {
            heightLineChart = GenerateLineChart(heights, _localized["ReportTitles_Height"]);   
        }

        byte[]? bloodPressureBarChart = null;
        if (bloodPressures.Count() > 0)
        {
            bloodPressureBarChart = GenerateStackedBarChart(bloodPressures, _localized["ReportTitles_BloodPressure"]);
        }

        var stream = new MemoryStream();
        uint rowCount = 1;
        
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);

                page.Header()
                    .PaddingBottom(20)
                    .Row(row =>
                    {
                        row.RelativeItem()
                            .Text(string.Format(_localized["ReportTitles_PatientID"], patient.Id))
                            .SemiBold()
                            .FontSize(10);
                    });

                page.Content()
                    .Table(table =>
                    {
                        var now = DateTime.UtcNow;
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();

                        });

                        table.Cell().Row(1).Column(1).ColumnSpan(2).PaddingBottom(35)
                            .Text(string.Format(_localized["ReportTitles_Patient"], patient.FirstName, patient.LastName))
                            .SemiBold()
                            .FontSize(16)
                            .FontColor(Colors.Blue.Medium);

                        if (firstBmi != null)
                        {
                            rowCount++;

                            // TODO: Remove force unwrap
                            var formattedDate = firstBmiDate!.Value.ToString(_localized["DateFormatting_Date"]);

                            table.Cell().Row(rowCount).Column(1).AlignRight().PaddingRight(10).Text(string.Format(_localized["ReportTitles_BMIStart"], formattedDate, Math.Round((double)firstBmi))).FontSize(12);
                        }

                        if (lastBmi != null)
                        {
                            rowCount++;
                            // TODO: Remove force unwrap
                            var formattedDate = lastBmiDate!.Value.ToString(_localized["DateFormatting_Date"]);
                            table.Cell().Row(rowCount).Column(2).AlignLeft().PaddingLeft(10).Text(string.Format(_localized["ReportTitles_BMIEnd"], formattedDate, Math.Round((double)lastBmi))).FontSize(12);
                        }

                        if (temperatureBarChart != null)
                        {
                            rowCount++;
                            table.Cell().Row(rowCount).Column(1).ColumnSpan(2).PaddingTop(50).Image(temperatureBarChart);
                        }
                        
                        if (weightLineChart != null)
                        {
                            rowCount++;
                            table.Cell().Row(rowCount).Column(1).ColumnSpan(2).Image(weightLineChart);
                        }
                        
                        if (heightLineChart != null)
                        {
                            rowCount++;
                            table.Cell().Row(rowCount).Column(1).ColumnSpan(2).Image(heightLineChart);
                        }

                        if (bloodPressureBarChart != null)
                        {
                            rowCount++;
                            table.Cell().Row(rowCount).Column(1).ColumnSpan(2).Image(bloodPressureBarChart);
                        }
                    });
            });
        }).GeneratePdf(stream);

        var now = DateTime.UtcNow;
        var formattedDate = now.ToString(_localized["DateFormatting_DateTime"]);
        var reportTitle = $"{patient.LastName}_{patient.FirstName}_{formattedDate}.pdf";
        var response = new ReportResponse(stream.ToArray(), reportTitle);

        return response;
    }

    private static byte[] GenerateLineChart(IEnumerable<(DateTime, double)> values, string title)
    {
        var xValues = "";
        var yValues = "";
        var yLabels = "0:||";
        var minVal = Double.MaxValue;
        var maxVal = Double.MinValue;
        var i = 1;
        foreach (var item in values)
        {
            xValues = xValues + i + ",";
            yValues = yValues + item.Item2.ToString(CultureInfo.InvariantCulture) + ",";
            yLabels = yLabels + item.Item1.Day + "/" + item.Item1.Month + "/" + item.Item1.Year + "|";

            if (item.Item2 > maxVal) maxVal = item.Item2;
            if (item.Item2 < minVal) minVal = item.Item2;

            i++;
        }

        var difference = maxVal - minVal;

        return new ImageCharts()
            // chart type, line chart
            .cht("lxy")
            // chart image size
            .chs("999x999")
            // Data points
            .chd($"t:{xValues}|" +
                 $"{yValues}")
            // Labels, dates of metrics
            .chxl(yLabels)
            // What range to show at the start, we specify just below and just above the min/ max values
            .chxr(
                $"1,{Math.Round(minVal - difference, MidpointRounding.ToNegativeInfinity)},{Math.Round(maxVal + difference, MidpointRounding.ToPositiveInfinity)}")
            // Which axis to display
            .chxt("x,y")
            // Chart title
            .chtt(title.Replace(" ", "+"))
            // title colour and size
            .chts("000000,20")
            // grid lines
            .chg("1,1")
            .toBuffer();
    }
    
    private static byte[] GenerateBarChart(IEnumerable<(DateTime, double)> values, string title)
    {
        var yValues = "";
        var yLabels = "";
        var minVal = Double.MaxValue;
        var maxVal = Double.MinValue;
        foreach (var item in values)
        {
            yValues = yValues + item.Item2.ToString(CultureInfo.InvariantCulture) + "|";
            yLabels = yLabels + item.Item1.Day + "/" + item.Item1.Month + "/" + item.Item1.Year + "|";

            if (item.Item2 > maxVal) maxVal = item.Item2;
            if (item.Item2 < minVal) minVal = item.Item2;
        }

        var difference = maxVal - minVal;
        
        return new ImageCharts()
            // chart type bar chart
            .cht("bvg")
            // chart size
            .chs("999x999")
            // values of bar chart heights
            .chd($"t:{yValues}")
            // labels to display on bars
            .chl(yLabels)
            // range to display on Y axis
            .chxr(
            $"0,{Math.Round(minVal - difference, MidpointRounding.ToNegativeInfinity)},{maxVal}")
            // set which axis to show
            .chxt("y")
            // title text
            .chtt(title.Replace(" ", "+"))
            // title colour and size
            .chts("000000,20")
            // label formatting
            .chlps("anchor,start|clamp,true")
            // grid lines
            .chg("1,1")
            .toBuffer();
    }
    
    private static byte[] GenerateStackedBarChart(IEnumerable<(DateTime, string)> values, string title)
    {
        var yLabels = "";
        var lowValues = "";
        var highValues = "";
        var minVal = Double.MaxValue;
        var maxVal = Double.MinValue;
        foreach (var item in values)
        {
            yLabels = yLabels + item.Item1.Day + "/" + item.Item1.Month + "/" + item.Item1.Year + "|";

            var split = item.Item2.Split("/");
            var high = Int32.Parse(split.First());
            var low = Int32.Parse(split.Last());

            highValues += high + ",";
            lowValues += low + ",";

            if (high > maxVal) maxVal = high;

            if (low < minVal) minVal = low;
        }

        var yValues = highValues + "|" + lowValues;

        return new ImageCharts()
            // chart type bar chart
            .cht("bvs")
            // chart size
            .chs("999x999")
            // values of bar chart heights
            .chd($"t:{yValues}")
            // labels to display on bars
            .chl(yLabels)
            // set which axis to show
            .chxt("y")
            // title text
            .chtt(title.Replace(" ", "+"))
            // title colour and size
            .chts("000000,20")
            // label formatting
            .chlps("anchor,start|clamp,true")
            // grid lines
            .chg("1,1")
            .toBuffer();
    }
}