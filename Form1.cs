using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var tree = new QuadTree<int>(50, 0f, 0f, 1000f, 1000f);
            var quads = new List<Quad>();

            
            var firstQuad = new Quad(50f, 50f, 100f, 100f);
            //İlk quadı ağaca 0. pozisyonda ekliyoruz
            tree.Insert(0, ref firstQuad);
            quads.Add(firstQuad);
            
            var random = new Random();
            for (int i = 0; i < 1000; ++i)
            {
                var x = (float)random.NextDouble() * 950f;
                var y = (float)random.NextDouble() * 950f;
                var w = 10f + (float)random.NextDouble() * 40f;
                var h = 10f + (float)random.NextDouble() * 40f;
                var quad = new Quad(x, y, x + w, y + h);
                tree.Insert(quads.Count, ref quad);
                quads.Add(quad);
            }
            //Rastgele koordinat ve boyutlar oluşturulurarak 1000 quad ağaca eklendi.
            
            var collisions = new List<int>();
            //0. quad ile çakışan quadlar bulunacak:
            if (tree.FindCollisions(0, ref collisions))
            {

                for (int i = 0; i < collisions.Count; ++i)
                { MessageBox.Show("0. quad il ile çakışan quadın değeri: -> " + collisions[i].ToString());
                }
            }

            //Belli bir poazisyon ile çakışan quadlar bulunacak
            if (tree.SearchPoint(750f, 600f, ref collisions))
            {
                for (int i = 0; i < collisions.Count; ++i)
                { MessageBox.Show("750f, 600f noktası ile çakışan quadın değeri: -> " + collisions[i].ToString());
                }
            }

        }
    }
}
