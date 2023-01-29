using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using mshtml;
using System.Data;
using System.Data.SqlClient;
using IronPython.Hosting;
namespace Indexer
{
    
    class Program
    {    
        static void Main(string[] args)
        {
            
            Dictionary<string, List<int>> termsPos = new Dictionary<string,List<int>>();//dictionary to hold terms and positions
            List<string> tokens; // list of tokens obtained from tokenization
            List<string> stop = StopWords();//list of stopwords
            List<string> preStem = new List<string>();//list of non stemmed terms
            Porter2 stemmer = new Porter2();//object of porter class to stem words

            int count = 1;//document id
            string html = "";//string to hold the entire html document for later parsing

            while (count < 1501)
            {
                html = getText("f" + count);
                html = html.ToLower();
                tokens = Tokenization(html);
                for (int i = 0; i < tokens.Count; i++)
                {
                    if (!preStem.Contains(tokens[i])) //this just assisstant request to see preprocessed vs non preprocessed u can remove it and the List from the top
                        preStem.Add(tokens[i]);
                    string stemmed_word = stemmer.stem(tokens[i]);//word after being stemmed
                    if (!stop.Contains(stemmed_word))
                    {
                        if (termsPos.ContainsKey(stemmed_word))
                        {
                            termsPos[stemmed_word].Add(i);                          
                        }
                        else
                            termsPos.Add(stemmed_word, new List<int> { i });
                    }
                }
                preStem.Clear();
                termsPos.Clear();
                count++;
            }
           
        }
        static string getText(string file)
        {
            FileStream fs = new FileStream(file+".txt", FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            string rString = sr.ReadToEnd();
            sr.Close();
            fs.Close();
            string html = "";
            IHTMLDocument2 doc = new HTMLDocumentClass();
            doc.write(rString);
            IHTMLElement2 ie = (IHTMLElement2)doc.body;
            IHTMLElementCollection iec = ie.getElementsByTagName("p"); // all the text we get is from p tags only u can add more tags here if u want articles or H1s
            foreach (IHTMLElement el in iec)
            {
                html += el.innerText;
            }
            return html;
        }
        static List<string> Tokenization(string text)
        {
            string[] tokens = text.Split(' ',',','.');
            List<string> tks = tokens.ToList();
            //remove panctuation
            foreach(string s in tks)
            {
                for (int i = 0; i < s.Length; i++)
                {
                    if (char.IsPunctuation(s[i]))
                        s.Remove(i, 1);
                }
            }
            return tks;
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
    }
}
