using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StationPickerEmergencyData
{
    internal class Program
    {
        private static void Main(string[] args)
        {   
            try
            {
                var faresfile = "s:/FareLocationsRefData.xml";
                var stationfile = "s:/StationsRefData.xml";
                if (!File.Exists(faresfile) || !File.Exists(stationfile))
                {
                    throw new Exception($"This program relies on the S:\\ drive containing IDMS data." +
                        $"Either one or both the files {stationfile} and {faresfile} is missing.");
                }

                XDocument faredoc = XDocument.Load(faresfile);
                XDocument stationdoc = XDocument.Load(stationfile);

                var stationlist = (from station in stationdoc.Element("StationsReferenceData").Elements("Station")
                                   where (string)station.Element("UnattendedTIS") == "true" &&
                                   !string.IsNullOrWhiteSpace((string)station.Element("CRS")) &&
                                   (string)station.Element("OJPEnabled") == "true"
                                   join fare in faredoc.Element("FareLocationsReferenceData").Elements("FareLocation")
                                   on (string)station.Element("Nlc") equals (string)fare.Element("Nlc")
                                   where (string)fare.Element("UnattendedTIS") == "true"
                                   select new
                                   {
                                       CRS = (string)station.Element("CRS"),
                                       nlc = (string)fare.Element("Nlc"),
                                       Name = (string)fare.Element("OJPDisplayName"),
                                   }).Distinct();
                foreach (var station in stationlist)
                {
                    Console.WriteLine($"        new StationInfo {{Crs = \"{station.CRS}\", Nlc = \"{station.nlc}\", OJPDisplayName=\"{station.Name}\"}},");
                }
            }
            catch (Exception ex)
            {
                var codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                var progname = Path.GetFileNameWithoutExtension(codeBase);
                Console.Error.WriteLine(progname + ": Error: " + ex.Message);
            }

        }
    }
}
