using System.Collections.Generic;

namespace Ophelia.Web.UI.Controls
{
    public class List : WebControl
    {
        public void AddItems(List list)
        {
            for (int i = list.Controls.Count - 1; i >= 0; i--)
            {
                this.Controls.Add(list.Controls[i]);
            }
        }
        public ListItem AddItem(string Text, string URL, string LiCssClass = "", string ACssClass = "", int toIndex = -1, string OnClick = "")
        {
            var item = new ListItem();
            var link = new Link();
            link.Text = Text;
            link.URL = URL;
            item.CssClass = LiCssClass;
            link.CssClass = ACssClass;
            if (!string.IsNullOrEmpty(OnClick))
                link.OnClick = OnClick;
            item.Controls.Add(link);
            this.Controls.Add(item);
            if (toIndex > -1)
                this.MoveItemToIndex(item, toIndex);
            return item;
        }

        public ListItem AddItem(Link link, string LiCssClass = "", int toIndex = -1)
        {
            var item = new ListItem();
            item.Controls.Add(link);
            this.Controls.Add(item);
            item.CssClass = LiCssClass;
            if (toIndex > -1)
                this.MoveItemToIndex(item, toIndex);
            return item;
        }

        public ListItem AddModalLink(Link link, string ModalID, string LiCssClass = "", int toIndex = -1)
        {
            var item = new ListItem();
            item.Controls.Add(link);
            link.AddAttribute("data-toggle", "modal");
            link.AddAttribute("data-target", ModalID);
            item.CssClass = LiCssClass;
            this.Controls.Add(item);
            if (toIndex > -1)
                this.MoveItemToIndex(item, toIndex);
            return item;
        }

        public ListItem AddDropDown(Link link, string LiCssClass = "dropdown", int toIndex = -1)
        {
            var item = new ListItem();
            item.Controls.Add(link);
            link.AddAttribute("data-toggle", "dropdown");
            item.CssClass = LiCssClass;
            this.Controls.Add(item);
            if (toIndex > -1)
                this.MoveItemToIndex(item, toIndex);
            return item;
        }

        private void MoveItemToIndex(ListItem item, int index)
        {
            var temp = new List<ListItem>();
            foreach (ListItem tmpItem in this.Controls)
            {
                temp.Add(tmpItem);
            }
            this.Controls.Clear();
            for (int i = 0; i < temp.Count; i++)
            {
                if (i == index)
                    this.Controls.Add(item);
                if (item != temp[i])
                    this.Controls.Add(temp[i]);
            }
        }
        public List() : base(TextWriterTag.Ul)
        {

        }
    }
}
