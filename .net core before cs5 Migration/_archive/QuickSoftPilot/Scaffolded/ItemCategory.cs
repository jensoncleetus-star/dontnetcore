using System;
using System.Collections.Generic;

namespace QuickSoftPilot.Scaffolded;

public partial class ItemCategory
{
    public long ItemCategoryId { get; set; }

    public string ItemCategoryName { get; set; }

    public long? Parent { get; set; }

    public string Description { get; set; }

    public int Editable { get; set; }

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
