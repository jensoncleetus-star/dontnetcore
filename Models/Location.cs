using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.Models
{
	public class Location
	{
		[Key]
		public int LocationID { set; get; }

		public string LocationCode { set; get; }

		public string LocationName { set; get; }

		public int StateID { set; get; }

	}
}