using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mcdonalds {
    class Database {
        #region Vars

        private string connection_string = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "mcdonalds.db");
        #endregion

        #region Get data

        public void get_all_product() {
            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(connection_string)) {

                // Get a collection (or create, if doesn't exist)
                var products = db.GetCollection<TProduct>("products");

                // Get all data
                var query = products.Find(Query.All());
                foreach (var data in query) {

                    Console.WriteLine(String.Format("{0,-10} | {1,-20} | {2,-30} | {3,-40} | {4,-50}", data.Id, data.Tag_id, data.Name, data.Quantity, data.Date));
                }
            }
        }

        public List<string> get_all_items_name() {
            List<string> names = new List<string>();

            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(connection_string)) {

                // Get a collection (or create, if doesn't exist)
                var products = db.GetCollection<TProduct>("products");

                // Get all names
                var query = products.Find(Query.All());
                foreach (var data in query) {
                    names.Add(data.Name);
                }
            }
            return names;
        }

        public string get_product_by_tag_id(string tag_id) {
            string name = string.Empty;

            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(connection_string)) {

                // Get a collection (or create, if doesn't exist)
                var products = db.GetCollection<TProduct>("products");

                // Get name by tag id
                name = products.FindOne(x => x.Tag_id == tag_id).Name.ToString();
            }
            return name;
        }
        #endregion

        #region Set data

        public void add_product(string tag_id, string name, int quatity) {

            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(connection_string)) {

                // Get a collection (or create, if doesn't exist)
                var products = db.GetCollection<TProduct>("products");


                // Create your new TProduct instance
                var product = new TProduct {
                    Tag_id = tag_id,
                    Name = name,
                    Quantity = quatity,
                    Date = DateTime.Now
                };

                // Insert new product in DB (Id will be auto-incremented)
                products.Insert(product);
            }
        }
        #endregion

        #region Remove datas

        public void Remove_items() {

            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(connection_string)) {

                // Get a collection (or create, if doesn't exist)
                var products = db.GetCollection<TProduct>("products");

                // Get all data
                var query = products.Find(Query.All());

                foreach (var data in query) {
                    products.Delete(data.Id);
                }
            }
        }
        #endregion
    }
    #region Table class

    class TProduct {
        public int Id { get; set; }
        public string Tag_id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public DateTime Date { get; set; }
    }
    #endregion

}
