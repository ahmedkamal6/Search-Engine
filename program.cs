using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace TF_IDF
{
    class Program
    {
        static List<Terms> terms = getTerms();
        static HashSet<int> doc_ids = Get_ids(terms);
        static Dictionary<Terms, double> termsAndTfIdf = new Dictionary<Terms, double>();
        struct Terms
        {
            public string term;
            public int f_id;
            public int freq;
            public string pos;
        }
        static void Main(string[] args)
        {
            foreach(var v in doc_ids)
            {
                List<Terms> current_doc = new List<Terms>();
                foreach(var term in terms)
                {
                    if (v == term.f_id)
                        current_doc.Add(term);
                    if (v < term.f_id)
                        break;
                }
                foreach(var term in current_doc)
                {
                    if (termsAndTfIdf.ContainsKey(term))
                        continue;
                    double tf = (double)term.freq / current_doc.Count;
                    HashSet<int> count = new HashSet<int>();
                    foreach (var t in terms)
                        if (t.term == term.term)
                            count.Add(t.f_id);
                    double idf = Math.Log((double)doc_ids.Count / count.Count)/Math.Log(2);
                    double tf_idf = tf * idf;
                    termsAndTfIdf.Add(term, tf_idf);
                }
            }
            saveRanks(termsAndTfIdf);
        }
        static void saveRanks(Dictionary<Terms,double> termsAndScores)
        {
            FileStream fs = new FileStream("TF-IDF.txt", FileMode.OpenOrCreate);
            StreamWriter sw = new StreamWriter(fs);
            foreach(KeyValuePair<Terms,double> kvp in termsAndScores)
            {
                sw.WriteLine(kvp.Key.term + "    " + kvp.Value);
            }
            sw.Close();
            fs.Close();
        }
        static HashSet<int> Get_ids(List<Terms> terms)
        {
            HashSet<int> res = new HashSet<int>();
            foreach (var term in terms)
                res.Add(term.f_id);
            return res;
        }
        static List<Terms> getTerms()
        {
            FileStream fs = new FileStream("D:/Final Year/Second term/IR/labs/section 5/indexedTermsFile.txt", FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            List<Terms> terms = new List<Terms>();
            while (sr.Peek() != -1)
            {
                string s = sr.ReadLine();
                string[] s1 = s.Split('+');
                Terms term;
                term.term = s1[0];
                term.f_id = Convert.ToInt32(s1[1]);
                term.freq = Convert.ToInt32(s1[2]);
                term.pos = s1[3];
                terms.Add(term);
            }
            return terms;
        }
    }
}
