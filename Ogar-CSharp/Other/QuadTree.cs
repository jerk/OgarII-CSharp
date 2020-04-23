using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace Ogar_CSharp
{
    public interface IQuadItem
    {
        public RectangleF Range { get; }
    }
    public class QuadTree<T> where T : IQuadItem
    {
        private static readonly Dictionary<T, QuadTree<T>> roots = new Dictionary<T, QuadTree<T>>(3000);
        public QuadTree<T> root;
        public int level;
        public int maxLevel;
        public int maxItems;
        public RectangleF range;
        public List<T> items;
        public readonly QuadTree<T>[] branches = new QuadTree<T>[4];
        public bool hasSplit = false;
        public QuadTree(RectangleF range, int maxLevel, int maxItems, QuadTree<T> root)
        {
            this.root = root;
            this.level = root != null ? root.level + 1 : 1;
            this.maxLevel = maxLevel;
            this.maxItems = maxItems;
            this.range = range;
            items = new List<T>(this.maxItems);
        }
        public void Destroy()
        {
            for (int i = 0, l = this.items.Count; i < l; i++)
                roots[this.items[i]] = null;
            if (this.hasSplit)
                for (int i = 0; i < 4; i++)
                    this.branches[i].Destroy();
        }
        public void Insert(T item)
        {
            var quad = this;
            while (true)
            {
                if (!quad.hasSplit)
                    break;
                var quadrant = quad.GetQuadrant(item.Range);
                if (quadrant == -1)
                    break;
                quad = quad.branches[quadrant];
            }
            roots[item] = quad;
            quad.items.Add(item);
            quad.Split();
        }
        public void Update(T item)
        {
            var oldQuad = roots[item];
            var newQuad = oldQuad;
            while (true)
            {
                if (newQuad.root == null)
                    break;
                newQuad = newQuad.root;
                if (Misc.FullyIntersects(newQuad.range, item.Range))
                    break;

            }
            while (true)
            {
                if (!newQuad.hasSplit) 
                    break;
                var quadrant = newQuad.GetQuadrant(item.Range);
                if (quadrant == -1) 
                    break;
                newQuad = newQuad.branches[quadrant];
            }
            if (oldQuad == newQuad) 
                return;
            oldQuad.items.Remove(item);
            newQuad.items.Add(item);
            roots[item] = newQuad;
            oldQuad.Merge();
            newQuad.Split();
        }
        public void Remove(T item)
        {
            var quad = roots[item];
            quad.items.Remove(item);
            roots[item] = null;
            quad.Merge();
        }
        public void Merge()
        {
            var quad = this;
            while (quad != null)
            {
                if (!quad.hasSplit)
                {
                    quad = quad.root;
                    continue;
                }
                QuadTree<T> branch;
                for (int i = 0; i < 4; i++)
                    if ((branch = quad.branches[i]).hasSplit || branch.items.Count > 0)
                        return;
                quad.hasSplit = false;
                quad.branches[0] = null;
                quad.branches[1] = null;
                quad.branches[2] = null;
                quad.branches[3] = null;
            }
        }
        public void Split()
        {
            if (hasSplit || level > maxLevel || items.Count < maxItems)
                return;
            hasSplit = true;
            var x = range.X;
            var y = range.Y;
            var hw = range.Width / 2;
            var hh = range.Height / 2;
            branches[0] = new QuadTree<T>(new RectangleF(x - hw, y - hh, hw, hh), maxLevel, maxItems, this);
            branches[1] = new QuadTree<T>(new RectangleF(x + hw, y - hh, hw, hh), maxLevel, maxItems, this);
            branches[2] = new QuadTree<T>(new RectangleF(x - hw, y + hh, hw, hh), maxLevel, maxItems, this);
            branches[3] = new QuadTree<T>(new RectangleF(x + hw, y + hh, hw, hh), maxLevel, maxItems, this);
            for (int i = 0, l = this.items.Count, quadrant; i < l; i++)
            {
                quadrant = this.GetQuadrant(this.items[i].Range);
                if (quadrant == -1) continue;
                roots[this.items[i]] = null;
                this.branches[quadrant].Insert(this.items[i]);
                this.items.RemoveAt(i);
                i--;
                l--;
            }
        }
        public int GetQuadrant(RectangleF a)
        {
            var quad = Misc.GetQuadFullIntersect(a, this.range);
            if (quad.t)
            {
                if (quad.l)
                    return 0;
                if (quad.r)
                    return 1;
            }
            if (quad.b)
            {
                if (quad.l) 
                    return 2;
                if (quad.r) 
                    return 3;
            }
            return -1;
        }
        public void Search(RectangleF range, Action<T> callback)
        {
            T item;
            for (int i = 0, l = items.Count; i < l; i++)
                if (Misc.Intersects(range, (item = this.items[i]).Range))
                    callback(item);
            if (!hasSplit)
                return;
            var quad = Misc.GetQuadIntersect(range, this.range);
            if (quad.t)
            {
                if (quad.l)
                    branches[0].Search(range, callback);
                if (quad.r)
                    branches[1].Search(range, callback);
            }
            if (quad.b)
            {
                if (quad.l)
                    branches[2].Search(range, callback);
                if (quad.r)
                    branches[3].Search(range, callback);
            }
        }
        public bool ContainsAny(RectangleF range, Func<T, bool> selector)
        {
           T item;
            for (int i = 0, l = this.items.Count; i < l; i++)
                if (Misc.Intersects(range, (item = this.items[i]).Range) && (selector == null || selector(item)))
                    return true;
            if (!this.hasSplit) return false;
            var quad = Misc.GetQuadIntersect(range, this.range);
            if (quad.t)
            {
                if (quad.l && branches[0] != null && this.branches[0].ContainsAny(range, selector))
                    return true;
                if (quad.r && branches[1] != null && this.branches[1].ContainsAny(range, selector))
                    return true;
            }
            if (quad.b)
            {
                if (quad.l && branches[2] != null && this.branches[2].ContainsAny(range, selector))
                    return true;
                if (quad.r && branches[3] != null && this.branches[3].ContainsAny(range, selector))
                    return true;
            }
            return false;
        }
        public int GetItemCount()
        {
            if (!this.hasSplit)
                return this.items.Count;
            else
                return this.items.Count +
                    this.branches[0].GetItemCount() +
                    this.branches[1].GetItemCount() +
                    this.branches[2].GetItemCount() +
                    this.branches[3].GetItemCount();
        }
        public int GetBranchCount()
        {
            if(this.hasSplit)
                return 1 +
                      this.branches[0].GetBranchCount() + this.branches[1].GetBranchCount() +
                this.branches[2].GetBranchCount() + this.branches[3].GetBranchCount();
            return 1;
        }
    }
}
