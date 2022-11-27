using ClubParkrunResultsProcessor.Models;
using HtmlAgilityPack;
using System.Text.Json;

const string ConsolidatedResultsPageUrl = "https://www.parkrun.com/results/consolidatedclub/";
const string ClubCodeQuerystringKey = "clubNum";
const string EventDateQueryStringKey = "eventdate"; //eventdate=2021-12-25 - NOTE only append to query if an EventDate is specified

const string ClubCode = "2283";
const string ClubName = "West End Runners";
const string ClubNameAbbreviated = "WER";
const string EventDate = ""; // "2021-12-25";

List<string> Exclusions = new List<string>()
{       
    "Zoe Webster",
    "Morven Burden",
    "Elaine O'Connor",
    "Michael David Guy",
    "Vincent Ly",
    "Emma Forster",
    "Ali Robins",
    "Paul Martin",
    "Kieron Fletcher",
    "Darrell Gray",
    "Sophie Devine",
    "Thomas Fincham",
    "David Orton",
    "Kathryn Hall",
    "Svenja BETHKE",
    "Sara TOMASSINI",
    "Joe Parker",
    "Taylor Robinson",
    "Philip MATHEW",
    "Kieron Fletcher",
    "Martin CHAMBERLAIN",
    "Keiron FLETCHER",
    "Shaun NEWBOLD",
    "Caroline Smith",
    "Jane French",
    "Louise Shearsby"
};

//const string ClubCode = "17946";
//const string ClubName = "Leicester Triathlon Club";
//const string ClubNameAbbreviated = "LTC";

//const string ClubCode = "1824";
//const string ClubName = "Beaumont RC";
//const string ClubNameAbbreviated = "BRC";


//const string ClubCode = "1814";
//const string ClubName = "Barrow Runners";
//const string ClubNameAbbreviated = "BR";



const string NonClubRunnersClubName = "-";

const string OutputDirectory = @"D:\\Work\\Projects\\Personal Projects\\Parkrun Results Scraper\\Output";

List<Parkrun> parkruns = new List<Parkrun>();

//if (File.Exists("testdata2.json"))
//{
//    string testData = File.ReadAllText("testdata.json");
//    parkruns = System.Text.Json.JsonSerializer.Deserialize<List<Parkrun>>(testData);

//}

#region Import

HtmlWeb web = new HtmlWeb();

string eventDateQueryStringFilter = "";

if (!String.IsNullOrEmpty(EventDate))
    eventDateQueryStringFilter = $"&{EventDateQueryStringKey}={EventDate}";

//https://www.parkrun.com/results/consolidatedclub/?clubNum=2283

HtmlDocument document = web.Load($"{ConsolidatedResultsPageUrl}?{ClubCodeQuerystringKey}={ClubCode}{eventDateQueryStringFilter}");

HtmlNode resultsWrapper = document.DocumentNode.SelectNodes("//div[@class='results-wrapper']").First();
HtmlNode floatLeft = resultsWrapper.SelectNodes("//div[@class='floatleft']").First();
var h2TitleNodes = resultsWrapper.SelectNodes("//h2").ToList();

foreach (var item in h2TitleNodes)
{
    var parkrun = new Parkrun() { Title = item.InnerText, ParticpantsDescription = item.NextSibling.InnerText };

    var linkToParkrun = item.NextSibling.NextSibling.ChildNodes.First();
    parkrun.LinkToParkrunResults = linkToParkrun.Attributes.First().Value;

    var resultsTable = item.NextSibling.NextSibling.NextSibling.NextSibling;

    for (int i = 1; i < resultsTable.ChildNodes.Count; i++)
    {
        string runnerName = resultsTable.ChildNodes[i].ChildNodes[2].InnerText;

        if (Exclusions.Any(e => e.Equals(runnerName, StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine($"Excluded {runnerName}");
        }
        else
        {
            var result = new Result()
            {
                Position = Convert.ToInt32(resultsTable.ChildNodes[i].ChildNodes[0].InnerText),
                GenderPosition = Convert.ToInt32(resultsTable.ChildNodes[i].ChildNodes[1].InnerText),
                Name = resultsTable.ChildNodes[i].ChildNodes[2].InnerText,
                LinkToRunner = resultsTable.ChildNodes[i].ChildNodes[2].ChildNodes.First().Attributes.First().Value,
                Club = resultsTable.ChildNodes[i].ChildNodes[3].InnerText.Trim().Equals(ClubName, StringComparison.InvariantCultureIgnoreCase) ? ClubNameAbbreviated : NonClubRunnersClubName,
                Time = resultsTable.ChildNodes[i].ChildNodes[4].InnerText
            };
            parkrun.Results.Add(result);
        }

    }
    if (parkrun.Results.Count > 0 && parkrun.Results.Any(r => !String.Equals(r.Club, NonClubRunnersClubName, StringComparison.InvariantCultureIgnoreCase)))
    {
        parkruns.Add(parkrun);
    }
    else
    {
        Console.WriteLine($"Excluded {parkrun.Title} as it has no valid club runners");
    }

}

#endregion

#region Export

var outputDoc = new HtmlDocument();
var outputDiv = HtmlNode.CreateNode("<div></div>");
outputDoc.DocumentNode.AppendChild(outputDiv);

foreach (var parkrun in parkruns)
{
    var parkrunTitle = HtmlNode.CreateNode($"<div><a href='{parkrun.LinkToParkrunResults}'>{parkrun.Title}</a></div>");
    outputDiv.AppendChild(parkrunTitle);
    var participantsDescription = HtmlNode.CreateNode($"<div>{parkrun.ParticpantsDescription}</div>");
    outputDiv.AppendChild(participantsDescription);

    var resultsTable = HtmlNode.CreateNode("<table></table>");
    outputDiv.AppendChild(resultsTable);

    var titleRow = HtmlNode.CreateNode("<tr></tr>");
    resultsTable.AppendChild(titleRow);

    titleRow.AppendChild(HtmlNode.CreateNode($"<th>Pos</th>"));
    titleRow.AppendChild(HtmlNode.CreateNode($"<th>Gender Pos</th>"));
    titleRow.AppendChild(HtmlNode.CreateNode($"<th>Name</th>"));
    titleRow.AppendChild(HtmlNode.CreateNode($"<th>Club</th>"));
    titleRow.AppendChild(HtmlNode.CreateNode($"<th>Time</th>"));

    foreach (var result in parkrun.Results)
    {
        var resultRow = HtmlNode.CreateNode("<tr></tr>");
        resultsTable.AppendChild(resultRow);

        resultRow.AppendChild(HtmlNode.CreateNode($"<td>{result.Position}</td>"));
        resultRow.AppendChild(HtmlNode.CreateNode($"<td>{result.GenderPosition}</td>"));
        resultRow.AppendChild(HtmlNode.CreateNode($"<td><a href='{result.LinkToRunner}'>{result.Name}</a></td>"));
        resultRow.AppendChild(HtmlNode.CreateNode($"<td>{result.Club}</td>"));
        resultRow.AppendChild(HtmlNode.CreateNode($"<td>{result.Time}</td>"));
    }

    outputDiv.AppendChild(HtmlNode.CreateNode("<br/>"));

}

var fileName = $"ParkrunResults-{ClubName}-{DateTime.Now:yyyy-MM-dd HH-mm-ss}.htm";

File.WriteAllText(Path.Combine(OutputDirectory, fileName), outputDiv.OuterHtml);

#endregion

Console.WriteLine("Finished!");
