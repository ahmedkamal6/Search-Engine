using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Search
{
    class Program
    {
        static List<string> stops = StopWords();
        static Porter stemmer = new Porter();
        static Dictionary<int, string> URL = getURLs();
        static List<Terms> terms = getTerms();
        static HashSet<int> doc_ids = Get_ids(terms);
        static List<string> res_URLs = new List<string>();
        struct Terms
        {
            public string term;
            public int f_id;
            public int freq;
            public string pos;
        }
        static HashSet<int> Get_ids(List<Terms> terms)
        {
            HashSet<int> res = new HashSet<int>();
            foreach (var term in terms)
                res.Add(term.f_id);
            return res;
        }
        static void Main(string[] args)
        {   
            string searchTerm = Console.ReadLine();//input
            List<string> searchWords = Processing(searchTerm);
            SortedDictionary<int, int> prio = new SortedDictionary<int, int>();//rank ids according to frequency or distance between words

            if (searchWords.Count == 1)
                prio = exact_search(searchWords); // single word search also uses frequency llike exact phrase query
            else if (searchTerm.StartsWith("\""))
               prio = exact_search(searchWords);
            else
                prio = search(searchWords);//multiword search uses min distance between words
            if(prio.Count == 0)
            {
                Console.WriteLine("no result found");
                return;
            }    
            var sortedDict = from entry in prio orderby entry.Value ascending select entry;//Linq to sort the dictionary based on freq or dist
            foreach (var kvp in sortedDict)
            {
                res_URLs.Add(URL[kvp.Key]);
            }
            foreach (var v in res_URLs) // returned URLs
                Console.WriteLine(v);
        }
        static SortedDictionary<int, int> exact_search(List<string> searchWords)
        {
            SortedDictionary<int, int> prio = new SortedDictionary<int, int>();
            for (int s = 0; s < searchWords.Count; s++) //remove " becasue it's not present in the indexer file
            {
                searchWords[s] = searchWords[s].Replace("\"", "");
                searchWords[s] = searchWords[s].Replace(searchWords[s], stemmer.stem(searchWords[s]));
                Console.WriteLine(searchWords[s]);
            }
            int min_freq; // the freq of all search terms in a single doc
            foreach (var id in doc_ids)
            {
                min_freq = 999999;
                List<Terms> currentDoc = new List<Terms>(); 
                foreach (var term in terms)//get terms inside each document
                {
                    if (term.f_id == id)
                        currentDoc.Add(term);
                    if (term.f_id > id)
                        break;
                }

                List<List<int>> id_positions = terms_positions_in_doc(searchWords, currentDoc, ref min_freq);//List of Lists of positions for every search term present in the doc
                if (id_positions.Count < searchWords.Count) //if this is false it means that 1 or more terms is not found in this document
                    continue;
                int m = 0, n = 0;
                
                while (m < min_freq && n < min_freq)//check if the search terms are one after the other
                {
                    bool freqInc = true;
                    for (int i = 0; i < id_positions.Count - 1; i++)
                    {

                        if (id_positions[i + 1][m] - id_positions[i][n] != 1)
                            if (id_positions[i + 1][m] > id_positions[i][n])
                            {
                                n++;
                                freqInc = false;
                                break;
                            }
                            else
                            {
                                m++;
                                freqInc = false;
                                break;
                            }
                    }
                    if (freqInc)//if this is false it means they are not one after the other 
                    {
                        if (prio.ContainsKey(id))
                            prio[id] += 1;
                        else
                            prio.Add(id, 1);
                        m++; n++;

                    }
                }
            }
            return prio;
        }
        static List<List<int>> terms_positions_in_doc(List<string> searchWords,List<Terms> currentDoc,ref int min_freq)
        {
            List<List<int>> id_positions = new List<List<int>>();
            foreach (var word in searchWords)
            {
                foreach (var term in currentDoc)
                {
                    List<int> current = new List<int>();
                    if (word == term.term)
                    {
                        string[] stringPositions = term.pos.Split(' ');
                        if (stringPositions.Length == 2)
                            current.Add(Convert.ToInt32(term.pos));
                        else
                        {
                            foreach (var position in stringPositions)
                            {
                                if (position == "")
                                    continue;
                                current.Add(Convert.ToInt32(position));
                            }
                        }
                        if (min_freq > current.Count)
                            min_freq = current.Count;
                        id_positions.Add(current);
                    }
                }
            }
            return id_positions;
        }
        static SortedDictionary<int,int> search(List<string> searchWords)
        {
            SortedDictionary<int, int> prio = new SortedDictionary<int, int>();
            for (int s = 0; s < searchWords.Count; s++)
            {
                searchWords[s] = searchWords[s].Replace(searchWords[s], stemmer.stem(searchWords[s]));
                Console.WriteLine(searchWords[s]);
            }
            int min_freq;
            foreach (var id in doc_ids)
            {
                min_freq = 999999;
                List<Terms> currentDoc = new List<Terms>();
                foreach (var term in terms)
                {
                    if (term.f_id == id)
                        currentDoc.Add(term);
                    if (term.f_id > id)
                        break;
                }
                List<List<int>> id_positions = terms_positions_in_doc(searchWords, currentDoc, ref min_freq);
                if (id_positions.Count < searchWords.Count)
                    if (id_positions.Count < searchWords.Count)
                    continue;
                int m = 0, n = 0;               
                int minDist = 999999;
                while (m < min_freq && n < min_freq)
                {
                    bool bigBreak = false;
                    for (int i = 0; i < id_positions.Count - 1; i++)
                    {

                        if (!(id_positions[i + 1][m] > id_positions[i][n]))
                        {
                            int p = m;
                            while (!(id_positions[i + 1][p] > id_positions[i][n]))
                            {
                                p++;
                                if (p > id_positions[i + 1].Count - 1)
                                {
                                    bigBreak = true;
                                    break;
                                }
                            }
                            if (bigBreak)
                                break;
                            if (minDist > (id_positions[i + 1][p] - id_positions[i][n]))
                                minDist = id_positions[i + 1][p] - id_positions[i][n];
                        }
                        else
                        {
                            if (minDist > (id_positions[i + 1][m] - id_positions[i][n]))
                                minDist = id_positions[i + 1][m] - id_positions[i][n];
                        }
                    }
                    if (bigBreak)
                        break;
                    n++;
                    m++;
                }
                if (minDist == 999999)
                    continue;
                prio.Add(id, minDist);
            }
            return prio;
        }
        static List<string> Processing(string text)
        {
            string[] tokens = text.Split(' ', ',', '.');
            List<string> tks = tokens.ToList();
            //remove stopwords
            List<string> noStops = new List<string>();
            foreach(var s in tks)
            {
                if(stops.Contains(s))
                    continue;
                noStops.Add(s);
            }
            //remove panctuation
            foreach (string s in noStops)
            {
                for (int i = 0; i < s.Length; i++)
                {
                    if (char.IsPunctuation(s[i]))
                        s.Remove(i, 1);
                }
            }
            return noStops;
        }
        static List<string> StopWords()
        {
            FileStream fs = new FileStream("stops.txt", FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            List<string> stop = new List<string>();
            int count = 0;
            while (sr.Peek() != -1)
            {
                stop.Add(sr.ReadLine());
                count++;
            }
            sr.Close();
            fs.Close();
            return stop;
        }
        static Dictionary<int, string> getURLs()
        {
            Dictionary<int, string> URL = new Dictionary<int, string>();
            FileStream fs = new FileStream("D:/Final Year/Second term/IR/section 5/Urls.txt", FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            int count = 1;
            while (sr.Peek() != -1)
            {
                string s = sr.ReadLine();
                string[] s1 = s.Split('+');
                URL.Add(count, s1[1]);
                count++;
            }
            sr.Close();
            fs.Close();
            return URL;
        }
        static List<Terms> getTerms()
        {
            FileStream fs = new FileStream("D:/Final Year/Second term/IR/section 5/indexedTermsFile.txt", FileMode.Open);
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
