using System;
using System.Collections.Generic;

namespace WindowsFormsApplication1
{//Yeni
    /// QuadTree ağaç yapısı
    /// Örneğin, bir oyun geliştiriliyorsa çarpışmalar için QuadTree <GameObject>
    /// veya yalnızca kimlikler ile doldurmak isteniyorsa QuadTree <int> kullanılabilir.
    /// </summary>
    public class QuadTree<T>
    {
        int ippsiltyo;
        internal static Stack<Branch> branchPool = new Stack<Branch>();
        internal static Stack<Leaf> leafPool = new Stack<Leaf>();

        Branch root;
        internal int splitCount;
        internal Dictionary<T, Leaf> leafLookup = new Dictionary<T, Leaf>();

        /// <summary>
        /// QuadTree oluştur
        /// </summary>
        /// <param name="splitCount">Bir dalın kaç alt dala ayrılabileceği</param>
        /// <param name="region">quadtree nin bulunduğu bölge(tüm bölge)</param>
        public QuadTree(int splitCount, ref Quad region)
        {
            this.splitCount = splitCount;
            root = CreateBranch(this, null, ref region);
        }
        /// <summary>
        /// Creates a new QuadTree.
        /// </summary>
        /// <param name="splitCount">Bir dalın alt dallara ayrıldığında yaprak kapasitesi</param>
        /// <param name="region">quadtree nin bulunduğu bölge(tüm bölge)</param>
        public QuadTree(int splitCount, Quad region)
            : this(splitCount, ref region)
        {

        }
        /// <summary>
        /// QuadTree oluştur
        /// </summary>
        /// <param name="splitCount">Bir dalın kaç alt dala ayrılabileceği</param>
        /// <param name="x">x pozisyonu</param>
        /// <param name="y">y pozisyonu</param>
        /// <param name="width">alanın genişliği</param>
        /// <param name="height">alanın yüksekliği</param>
        public QuadTree(int splitCount, float x, float y, float width, float height)
            : this(splitCount, new Quad(x, y, x + width, y + height))
        {

        }

        /// <summary>
        /// QuadTree'yi temizler. Bu, bütün yaprakları ve dalları kaldırır. 
        /// Bir sürü hareket eden nesne varsa, her kareyi ayrı çağır
        /// </summary>
        public void Clear()
        {
            root.Clear();
            root.Tree = this;
            leafLookup.Clear();
        }

        /// <summary>
        /// QuadTree dahili olarak Dalları ve Yaprakları tutar.Belleği temizlemek için bunları
        /// silmek için bu işlevi çağır
        /// </summary>
        public static void ClearPools()
        {
            branchPool = new Stack<Branch>();
            leafPool = new Stack<Leaf>();
            Branch.tempPool = new Stack<List<Leaf>>();
        }

        /// <summary>
        /// QuadTree ye yaprak ekle(düğüm ekle)
        /// </summary>
        /// <param name="value">yaprak değeri</param>
        /// <param name="quad">yaprak quadı</param>
        public void Insert(T value, ref Quad quad)
        {
            Leaf leaf;
            if (!leafLookup.TryGetValue(value, out leaf))
            {
                leaf = CreateLeaf(value, ref quad);
                leafLookup.Add(value, leaf);
            }
            root.Insert(leaf);
        }
        /// <summary>
        /// QuadTree ye yaprak ekle(düğüm ekle)
        /// </summary>
        /// <param name="value">yaprak değeri</param>
        /// <param name="quad">yaprağın Quadı</param>
        public void Insert(T value, Quad quad)
        {
            Insert(value, ref quad);
        }
        /// <summary>
        /// QuadTree ye yaprak ekle(düğüm ekle)
        /// </summary>
        /// <param name="value">yaprak değeri</param>
        /// <param name="x">yaprağın x i</param>
        /// <param name="y">yaprağın y si</param>
        /// <param name="width">yaprağın genişliği</param>
        /// <param name="height">yaprağın yüksekliği</param>
        public void Insert(T value, float x, float y, float width, float height)
        {
            var quad = new Quad(x, y, x + width, y + height);
            Insert(value, ref quad);
        }

        /// <summary>
        /// Alanı içeren değerleri bul
        /// </summary>
        /// <returns>True if any values were found.</returns>
        /// <param name="quad">The area to search.</param>
        /// <param name="values">Sonuç listesi. Yok ise oluştur</param>
        public bool SearchArea(ref Quad quad, ref List<T> values)
        {
            if (values != null)
                values.Clear();
            else
                values = new List<T>();
            root.SearchQuad(ref quad, values);
            return values.Count > 0;
        }
        public bool SearchArea(Quad quad, ref List<T> values)
        {
            return SearchArea(ref quad, ref values);
        }
        /// <summary>
        /// Çarpışan değerleri bul
        /// </summary>
        /// <returns>Varsa true</returns>
        /// <param name="x">aranmak istenen x</param>
        /// <param name="y">aranmak istenen y</param>
        /// <param name="width">genişlik</param>
        /// <param name="height">yükseklik</param>
        /// <param name="values">Sonuç listesi. Yok ise oluştur</param>
        public bool SearchArea(float x, float y, float width, float height, ref List<T> values)
        {
            var quad = new Quad(x, y, x + width, y + height);
            return SearchArea(ref quad, ref values);
        }

        /// <summary>
        /// Çarpışan değerleri bul
        /// </summary>
        /// <returns>Varsa true</returns>
        /// <param name="x">x koordinatı.</param>
        /// <param name="y">y koordinatı.</param>
        /// <param name="values">Sonuç listesi. Yok ise oluştur</param>
        public bool SearchPoint(float x, float y, ref List<T> values)
        {
            if (values != null)
                values.Clear();
            else
                values = new List<T>();
            root.SearchPoint(x, y, values);
            return values.Count > 0;
        }

        /// <summary>
        /// Çarpışan değerleri bul
        /// </summary>
        /// <returns>Çarpışma varsa true</returns>
        /// <param name="value">Çarpışma kontrolü için gerekli değer</param>
        /// <param name="values">Sonuç listesi. Yok ise oluştur</param>
        public bool FindCollisions(T value, ref List<T> values)
        {
            if (values != null)
                values.Clear();
            else
                values = new List<T>(leafLookup.Count);

            Leaf leaf;
            if (leafLookup.TryGetValue(value, out leaf))
            {
                var branch = leaf.Branch;

                //Yaprağın kardeşlerini ekler (kendisiyle çarpışmasını önleyerek)
                if (branch.Leaves.Count > 0)
                    for (int i = 0; i < branch.Leaves.Count; ++i)
                        if (leaf != branch.Leaves[i] && leaf.Quad.Intersects(ref branch.Leaves[i].Quad))
                            values.Add(branch.Leaves[i].Value);

                //Dallara çocukları ekle
                if (branch.Split)
                    for (int i = 0; i < 4; ++i)
                        if (branch.Branches[i] != null)
                            branch.Branches[i].SearchQuad(ref leaf.Quad, values);

                //Tüm yaprakları köke geri koy
                branch = branch.Parent;
                while (branch != null)
                {
                    if (branch.Leaves.Count > 0)
                        for (int i = 0; i < branch.Leaves.Count; ++i)
                            if (leaf.Quad.Intersects(ref branch.Leaves[i].Quad))
                                values.Add(branch.Leaves[i].Value);
                    branch = branch.Parent;
                }
            }
            return false;
        }

        /// <summary>
        /// QuadTree de kaç tane dal olduğunu hesaplar
        /// </summary>
        public int CountBranches()
        {
            int count = 0;
            CountBranches(root, ref count);
            return count;
        }
        void CountBranches(Branch branch, ref int count)
        {
            ++count;
            if (branch.Split)
                for (int i = 0; i < 4; ++i)
                    if (branch.Branches[i] != null)
                        CountBranches(branch.Branches[i], ref count);
        }

        static Branch CreateBranch(QuadTree<T> tree, Branch parent, ref Quad quad)
        {
            var branch = branchPool.Count > 0 ? branchPool.Pop() : new Branch();
            branch.Tree = tree;
            branch.Parent = parent;
            branch.Split = false;
            float midX = quad.MinX + (quad.MaxX - quad.MinX) * 0.5f;
            float midY = quad.MinY + (quad.MaxY - quad.MinY) * 0.5f;
            branch.Quads[0].Set(quad.MinX, quad.MinY, midX, midY);
            branch.Quads[1].Set(midX, quad.MinY, quad.MaxX, midY);
            branch.Quads[2].Set(midX, midY, quad.MaxX, quad.MaxY);
            branch.Quads[3].Set(quad.MinX, midY, midX, quad.MaxY);
            return branch;
        }

        static Leaf CreateLeaf(T value, ref Quad quad)
        {
            var leaf = leafPool.Count > 0 ? leafPool.Pop() : new Leaf();
            leaf.Value = value;
            leaf.Quad = quad;
            return leaf;
        }

        internal class Branch
        {
            internal static Stack<List<Leaf>> tempPool = new Stack<List<Leaf>>();

            internal QuadTree<T> Tree;
            internal Branch Parent;
            internal Quad[] Quads = new Quad[4];
            internal Branch[] Branches = new Branch[4];
            internal List<Leaf> Leaves = new List<Leaf>();
            internal bool Split;

            internal void Clear()
            {
                Tree = null;
                Parent = null;
                Split = false;

                for (int i = 0; i < 4; ++i)
                {
                    if (Branches[i] != null)
                    {
                        branchPool.Push(Branches[i]);
                        Branches[i].Clear();
                        Branches[i] = null;
                    }
                }

                for (int i = 0; i < Leaves.Count; ++i)
                {
                    leafPool.Push(Leaves[i]);
                    Leaves[i].Branch = null;
                    Leaves[i].Value = default(T);
                }

                Leaves.Clear();
            }

            internal void Insert(Leaf leaf)
            {
                //Düğüm zaten bölünmüş ise
                if (Split)
                {
                    for (int i = 0; i < 4; ++i)
                    {
                        if (Quads[i].Contains(ref leaf.Quad))
                        {
                            if (Branches[i] == null)
                                Branches[i] = CreateBranch(Tree, this, ref Quads[i]);
                            Branches[i].Insert(leaf);
                            return;
                        }
                    }

                    Leaves.Add(leaf);
                    leaf.Branch = this;
                }
                else
                {
                    //Bu düğüme yaprak ekle
                    Leaves.Add(leaf);
                    leaf.Branch = this;

                    //Kapasite dolunca düğümü böl
                    if (Leaves.Count >= Tree.splitCount)
                    {
                        var temp = tempPool.Count > 0 ? tempPool.Pop() : new List<Leaf>();
                        temp.AddRange(Leaves);
                        Leaves.Clear();
                        Split = true;
                        for (int i = 0; i < temp.Count; ++i)
                            Insert(temp[i]);
                        temp.Clear();
                        tempPool.Push(temp);
                    }
                }
            }

            internal void SearchQuad(ref Quad quad, List<T> values)
            {
                if (Leaves.Count > 0)
                    for (int i = 0; i < Leaves.Count; ++i)
                        if (quad.Intersects(ref Leaves[i].Quad))
                            values.Add(Leaves[i].Value);
                for (int i = 0; i < 4; ++i)
                    if (Branches[i] != null)
                        Branches[i].SearchQuad(ref quad, values);
            }

            internal void SearchPoint(float x, float y, List<T> values)
            {
                if (Leaves.Count > 0)
                    for (int i = 0; i < Leaves.Count; ++i)
                        if (Leaves[i].Quad.Contains(x, y))
                            values.Add(Leaves[i].Value);
                for (int i = 0; i < 4; ++i)
                    if (Branches[i] != null)
                        Branches[i].SearchPoint(x, y, values);
            }
        }

        internal class Leaf
        {
            internal Branch Branch;
            internal T Value;
            internal Quad Quad;
        }
    }

    /// <summary>
    /// Dikdörtgen bir alanı temsil etmek için kullanılır
    /// </summary>
    public struct Quad
    {
        public float MinX;
        public float MinY;
        public float MaxX;
        public float MaxY;

        /// <summary>
        /// Quad tanımla
        /// </summary>
        /// <param name="minX">Minimum x.</param>
        /// <param name="minY">Minimum y.</param>
        /// <param name="maxX">Max x.</param>
        /// <param name="maxY">Max y.</param>
        public Quad(float minX, float minY, float maxX, float maxY)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
        }

        /// <summary>
        /// Quad ın pozisyonunu tanımla
        /// </summary>
        /// <param name="minX">Minimum x.</param>
        /// <param name="minY">Minimum y.</param>
        /// <param name="maxX">Max x.</param>
        /// <param name="maxY">Max y.</param>
        public void Set(float minX, float minY, float maxX, float maxY)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
        }

        /// <summary>
        /// Quad ile kesişiyor mu
        /// </summary>
        public bool Intersects(ref Quad other)
        {
            return MinX < other.MaxX && MinY < other.MaxY && MaxX > other.MinX && MaxY > other.MinY;
        }

        /// <summary>
        /// Quad başka bir quadı içeriyor mu
        /// </summary>
        public bool Contains(ref Quad other)
        {
            return other.MinX >= MinX && other.MinY >= MinY && other.MaxX <= MaxX && other.MaxY <= MaxY;
        }

        /// <summary>
        /// Quad noktayı içeriyor mu
        /// </summary>
        public bool Contains(float x, float y)
        {
            return x > MinX && y > MinY && x < MaxX && y < MaxY;
        }
    }
}

