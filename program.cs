using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using mshtml;
using System.Net;

namespace Crowler
{
    class Program
    {    
        static void Main(string[] args)
        {
            Queue<string> URIs = new Queue<string>();
            Dictionary<string, bool> visited = new Dictionary<string, bool>();
            URIs.Enqueue("https://www.wikipedia.com");
            int count = 0;
            string URI = "";
            while (count < (3000))
            {
                try
                {                   
                    URI = URIs.Dequeue();
                    WebRequest req = WebRequest.Create(URI);
                    WebResponse res = req.GetResponse();
                    Stream stream = res.GetResponseStream();
                    StreamReader sr2 = new StreamReader(stream);
                    string rString = sr2.ReadToEnd();
                    stream.Close();
                    sr2.Close();
                    res.Close();
                    IHTMLDocument2 doc = new HTMLDocumentClass();
                    doc.write(rString);
                    IHTMLElementCollection elements = doc.all;
                    IHTMLElementCollection elems = doc.links;
                    bool english = false;
                    foreach (IHTMLElement el in elements)
                    {
                        if (el.tagName == "HTML")
                        {
                            string Lang = (string)el.getAttribute("lang", 0);
                            if (Lang == "en")
                                english = true;
                        }
                    }
                    if (!english)
                        continue;
                    foreach (IHTMLElement el in elems)
                    {
                        string link = (string)el.getAttribute("href", 0);
                        if (!visited.ContainsKey(link) && link.StartsWith("https"))
                        {
                            URIs.Enqueue(link);
                            visited.Add(link, false);
                        }
                    }
                    visited[URI] = true;
                    count++;
                }
                catch (Exception a)
                {
                    Console.WriteLine(a.ToString());
                    continue;
                }
            }
        }
    }
}
