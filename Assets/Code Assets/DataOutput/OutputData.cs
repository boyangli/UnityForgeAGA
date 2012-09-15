using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace DataOutput {

    /// <summary>
    /// Full data on actions taken during a game. Provides list of all actions according to player.
    /// </summary>
    [Serializable]
    public class OutputData {

        public List<OutputAction> actions { get; set; }     // actions taken over battles
        public List<OutputSurvey> responses { get; set; }   // survey responses

        public OutputData() {
            this.actions = new List<OutputAction>();
            this.responses = new List<OutputSurvey>();
        }

        public OutputData(List<OutputAction> actions, List<OutputSurvey> responses) {
            this.actions = new List<OutputAction>();
            this.actions.AddRange(actions);

            this.responses = new List<OutputSurvey>();
            this.responses.AddRange(responses);
        }

        public void addAction(OutputAction oa) {
            actions.Add(oa);
        }

        public void addResponse(OutputSurvey os) {
            responses.Add(os);
        }

        /// <summary>
        /// Outputs all of this output data to a specified file in csv format.
        /// </summary>
        /// <param name="fname"></param>
        public void myToCSV(string actionFname, string surveyFname) {
            StreamWriter file = new StreamWriter(actionFname + ".csv", false);
            //public OutputAction(int playerID, int turnNum, int battleNum, string spell, double performance, string enemy, int enemyFightNum, string task) 
            file.WriteLine("playedID, turnNum, battleNum, spell, spellType, performance, enemy, enemyType, enemyFightNum, task");
            foreach (OutputAction oa in this.actions) {
                file.WriteLine(oa);
            }
            file.Close();

            StreamWriter file2 = new StreamWriter(surveyFname + ".csv", false);
            //public OutputAction(int playerID, int turnNum, int battleNum, string spell, double performance, string enemy, int enemyFightNum, string task) 
            file2.WriteLine("playedID, battleNum, q1, q2");
            foreach (OutputSurvey os in this.responses) {
                file2.WriteLine(os);
            }
            file2.Close();
        }

        /// <summary>
        /// Creates CSV strings from a list of objects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="separator"></param>
        /// <param name="objectlist"></param>
        /// <returns></returns>
        public static string ToCSV<T>(string separator, IEnumerable<T> objectlist) {
            Type t = typeof(T);
            //FieldInfo[] fields = t.GetFields();
            PropertyInfo[] properties = t.GetProperties();

            string header = String.Join(separator, properties.Select(f => f.Name).ToArray());

            StringBuilder csvdata = new StringBuilder();
            csvdata.AppendLine(header);

            foreach (var o in objectlist) {
                csvdata.AppendLine(ToCsvFields(separator, properties, o));
            }
            return csvdata.ToString();
        }

        /// <summary>
        /// Creates CSV string from given object with its known fields.
        /// </summary>
        /// <param name="separator"></param>
        /// <param name="properties"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string ToCsvFields(string separator, PropertyInfo[] properties, object o) {
            StringBuilder linie = new StringBuilder();

            foreach (var f in properties) {
                if (linie.Length > 0) {
                    linie.Append(separator);
                }
                var x = f.GetValue(o, null);

                if (x != null) {
                    linie.Append(x.ToString());
                }
            }

            return linie.ToString();
        }
    }

}
