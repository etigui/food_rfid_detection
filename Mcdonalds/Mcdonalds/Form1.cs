using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mcdonalds {
    public partial class Form1 : MetroFramework.Forms.MetroForm {
        public Form1() {
            InitializeComponent();
        }

        #region Vars

        List<Item<string, int>> items = new List<Item<string, int>>();
        private int last_index_selected = -1;
        private string last_name_selected = string.Empty;
        private static string com_port = "COM4";
        Database db = null;
        Rfid rfid = null;
        #endregion

        #region Init

        private void Form1_Load(object sender, EventArgs e) {
            this.Width = Screen.PrimaryScreen.WorkingArea.Width;
            this.Height = Screen.PrimaryScreen.WorkingArea.Height;
            db = new Database();
            db.get_all_product();
            Item_button_enable();
        }
        #endregion

        #region Button items (add)

        private void Add_item(string name) {

            // Check if the items exists => get quantity
            int quantity = Get_item_quantity(name);

            // Update or add item
            if (quantity != 0) {
                var item = items.Find(s => s.Name == name);
                int q = new int();
                q = quantity + 1;
                item.Quantity = q;
            } else {
                items.Add(new Item<string, int>(name, 1));
            }
            Update_list_items();
        }

        private void Update_list_items() {
            LB_items.Items.Clear();
            foreach (Item<string, int> item in items) {
                LB_items.Items.Add($"{"[" + item.Quantity + "x]"} {item.Name}");
            }
        }

        private void BT_bigmac_Click(object sender, EventArgs e) {
            Add_item("BigMac");
        }

        private void BT_wraps_Click(object sender, EventArgs e) {
            Add_item("Wraps");
        }

        private void BT_cola_Click(object sender, EventArgs e) {
            Add_item("CocaCola");
        }

        private void BT_cheeseburger_Click(object sender, EventArgs e) {
            Add_item("Cheeseburger");
        }

        private void BT_salad_Click(object sender, EventArgs e) {
            Add_item("Salads");
        }

        private void BT_sprite_Click(object sender, EventArgs e) {
            Add_item("Sprite");
        }

        private void BT_bigtasty_Click(object sender, EventArgs e) {
            Add_item("BigTasty");
        }

        private void BT_nuggets_Click(object sender, EventArgs e) {
            Add_item("Nuggets");
        }

        private void BT_fanta_Click(object sender, EventArgs e) {
            Add_item("Fanta");
        }

        private void BT_mcfish_Click(object sender, EventArgs e) {
            Add_item("McFish");
        }

        private void BT_fries_Click(object sender, EventArgs e) {
            Add_item("Fries");
        }

        private void BT_water_Click(object sender, EventArgs e) {
            Add_item("Water");
        }
        #endregion

        #region Increase/decrease/delete

        private void BT_remove_Click(object sender, EventArgs e) {

            modify_item(false);
        }

        private void BT_add_Click(object sender, EventArgs e) {
            modify_item(true);
        }

        private void BT_delet_Click(object sender, EventArgs e) {
            if (LB_items.SelectedIndex != -1) {
                items.RemoveAt(LB_items.SelectedIndex);
                Update_list_items();
            }
        }

        private void Check_index() {
            if (last_index_selected != -1) {

                // Check if last items index exists
                if ((LB_items.Items.Count - 1) >= last_index_selected) {

                    // Get current selected items
                    string current_name_selected = LB_items.Items[last_index_selected].ToString().Split(' ')[1];
                    if (current_name_selected == last_name_selected) {
                        LB_items.SetSelected(last_index_selected, true);
                    }
                }
            }
        }


        private string Get_item() {
            if (LB_items.SelectedIndex != -1) {
                return LB_items.Items[last_index_selected].ToString();
            }
            return string.Empty;
        }

        private void modify_item(bool increase) {

            // Keep focuse on index when increase or decrease items quantity
            Check_index();

            // Check if the item exists
            string item = Get_item();
            if (item != string.Empty) {

                // Get item name
                string item_name = item.Split(' ')[1];

                // Check if the items exists => get quantity
                int quantity = Get_item_quantity(item_name);

                // Find
                var it = items.Find(s => s.Name == item_name);

                //Update or remove item
                if (quantity == 1 && !increase) {
                    //Remove items
                    items.Remove(it);
                } else {
                    int q = new int();
                    if (increase) {
                        q = quantity + 1;
                    } else {
                        q = quantity - 1;
                    }
                    it.Quantity = q;

                }
                Update_list_items();
            }
        }

        private int Get_item_quantity(string name) {
            foreach (Item<string, int> item in items) {
                if (item.Name == name) {
                    return item.Quantity;
                }
            }
            return 0;
        }

        private void LB_items_SelectedIndexChanged(object sender, EventArgs e) {
            if (LB_items.SelectedIndex != -1) {
                last_index_selected = LB_items.SelectedIndex;
                last_name_selected = LB_items.Items[last_index_selected].ToString().Split(' ')[1];
            }
        }


        #endregion

        #region Tag id validation

        private void Tag_detected(object sender, TagDetectedEventArgs e) {
            Console.WriteLine($"Tag ID: {e.Tag_id}");
            new Thread(() => Validate_item_tag(e.Tag_id)).Start();
        }

        private void Validate_item_tag(string tag_id) {
            int index = 0;
            int quantity = 0;
            bool parse = false;

            // Get product name by tag id
            string name = db.get_product_by_tag_id(tag_id);

            // Iter all items in the listbox
            foreach (string d in LB_items.Items) {
                if (d.Split(' ')[1] == name) {
                    parse = int.TryParse(d.Split(' ')[0].Replace("[", "").Replace("]", "").Replace("x", ""), out quantity);
                    break;
                }
                index++;
            }

            // Check if the int was well parsed and if we find the item name
            if (parse) {

                // When all items are validated => green backgroud
                if (Check_all_validated()) {
                    LB_items.BackColor = Color.Green;
                    LB_items.ForeColor = Color.White;
                }

                // Check if extra item added
                if (quantity != 0) {

                    // Invoke Listbox cause ither thread
                    LB_items.Invoke((Action)delegate {
                        LB_items.Items[index] = $"[{quantity - 1 }x] {name}";
                    });
                } else {
                    MessageBox.Show("You add an extra item !", "Item error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private bool Check_all_validated() {
            int quantity = 0;
            int all = 0;
            bool parse = false;

            // Iter all items in the listbox
            foreach (string d in LB_items.Items) {
                parse = int.TryParse(d.Split(' ')[0].Replace("[", "").Replace("]", "").Replace("x", ""), out quantity);
                all = all + quantity;
            }

            // Check if all items are at 0x
            if ((all - 1) == 0) {
                return true;
            }
            return false;
        }

        private void BT_validate_Click(object sender, EventArgs e) {

            // Check if there is something to checkout
            if (LB_items.Items.Count != 0) {
                if (BT_validate.Text == "Validate") {
                    BT_validate.Text = "New client";

                    // Red color => not all validated
                    LB_items.BackColor = Color.Red;
                    LB_items.ForeColor = Color.White;
                    Pannel_update(false);

                    // Start detect tag id
                    rfid = new Rfid(com_port);
                    rfid.Tag_detected += Tag_detected;
                    if (!rfid.Open_read()) {
                        throw new ArgumentException("RFID fatal error");
                    }
                } else {

                    // Check if server want to start a new client
                    DialogResult dialogResult = MessageBox.Show("It might rest items. Wanna start new client ?", "New client", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes) {
                        items.Clear();
                        LB_items.Items.Clear();
                        LB_items.BackColor = Color.White;
                        LB_items.ForeColor = Color.Black;
                        rfid.Tag_detected -= Tag_detected;
                        rfid.Close();
                        Pannel_update(true);
                        Item_button_enable();
                        BT_validate.Text = "Validate";
                    }
                }
            }
        }

        private void Item_button_enable() {
            List<string> names = db.get_all_items_name();
            List<string> ctrl = new List<string> { "-", "+", "Validate", "Remove", "New client" };
            foreach (Control cont in this.Controls) {
                if (cont.HasChildren) {
                    foreach (Control contChild in cont.Controls) {
                        if (contChild.GetType() == typeof(MetroFramework.Controls.MetroButton)) {
                            if (!names.Contains(contChild.Text) && !ctrl.Contains(contChild.Text)) {
                                contChild.Enabled = false;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Control pannel

        private void Pannel_update(bool enable) {
            BT_add.Enabled = enable;
            BT_bigmac.Enabled = enable;
            BT_bigtasty.Enabled = enable;
            BT_cheeseburger.Enabled = enable;
            BT_cola.Enabled = enable;
            BT_delet.Enabled = enable;
            BT_fanta.Enabled = enable;
            BT_fries.Enabled = enable;
            BT_mcfish.Enabled = enable;
            BT_nuggets.Enabled = enable;
            BT_remove.Enabled = enable;
            BT_salad.Enabled = enable;
            BT_sprite.Enabled = enable;
            BT_water.Enabled = enable;
            BT_wraps.Enabled = enable;
        }
        #endregion
    }
    #region Item class

    public class Item<Items1, Items2> {

        public Item(Items1 name, Items2 quantity) {
            Name = name;
            Quantity = quantity;
        }

        public Items1 Name { get; set; }
        public Items2 Quantity { get; set; }

    }
    #endregion
}
