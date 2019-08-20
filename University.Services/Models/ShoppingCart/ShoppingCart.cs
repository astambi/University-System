namespace University.Services.Models.ShoppingCart
{
    using System.Collections.Generic;
    using System.Linq;

    public class ShoppingCart // per client
    {
        private readonly IList<CartItem> items;

        public ShoppingCart()
        {
            this.items = new List<CartItem>();
        }

        public IEnumerable<CartItem> Items => new List<CartItem>(this.items);

        public void AddItem(int courseId)
        {
            var item = this.GetItemById(courseId);
            if (item == null)
            {
                this.items.Add(new CartItem { CourseId = courseId });
            }
        }

        public void Clear()
            => this.items.Clear();

        public void RemoveItem(int courseId)
        {
            var item = this.GetItemById(courseId);
            if (item != null)
            {
                this.items.Remove(item);
            }
        }

        private CartItem GetItemById(int courseId)
            => this.items
            .Where(i => i.CourseId == courseId)
            .FirstOrDefault();
    }
}
