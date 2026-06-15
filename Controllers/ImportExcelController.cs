using System.Linq.Dynamic.Core;
using ApplicationUserManager = Microsoft.AspNetCore.Identity.UserManager<QuickSoft.Models.ApplicationUser>;
using ApplicationSignInManager = Microsoft.AspNetCore.Identity.SignInManager<QuickSoft.Models.ApplicationUser>;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class ImportExcelController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ImportExcelController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        // [QkAuthorize(Roles = "Dev,Item ExcelUpload")]
        public ActionResult Import()
        {
            return View();
        }
        public long addsupplier(string supcode,string suppliername)
        {

           if(suppliername!="")
            {
                var exist=db.Suppliers.Any(o => o.SupplierName == suppliername);
                if(exist)
                {
                    var supid = db.Suppliers.Where(o => o.SupplierName == suppliername).Select(o=>o.SupplierID).FirstOrDefault();
                    return supid;
                }
                Accounts ac = new Accounts();
                ac.Name = suppliername;
                ac.Alias = suppliername;
                ac.OpnBalance = 0;
               
                 ac.OpnBalanceCr = 0;
                ac.PrintName = suppliername;
                ac.CreatedDate = System.DateTime.Now;


                ac.Group = 14;
                db.Accountss.Add(ac);
                db.SaveChanges();
                var acid = ac.AccountsID;
                if (acid != 0)
                {
                    Supplier sp = new Supplier();
                    sp.Accounts = acid;
                    sp.SupplierName = suppliername;
                    sp.SupplierCode = supcode;
               
                    db.Suppliers.Add(sp);
                    return sp.SupplierID;
                        }
               else
                {
                    return 0;
                }
            }
            return 0;
        }
        [HttpPost]
        //   [QkAuthorize(Roles = "Dev,Item ExcelUpload")]
        public ActionResult Import(IFormFile file, string selecttype)
        {
            DataSet ds = new DataSet();
            if (Request.Form.Files["file"].Length > 0)
            {
                string fileExtension = System.IO.Path.GetExtension(Request.Form.Files["file"].FileName);
                if (fileExtension == ".xls" || fileExtension == ".xlsx")
                {

                    string Files = Request.Form.Files["file"].FileName;
                    Files = string.Concat(Path.GetFileNameWithoutExtension(Files), DateTime.Now.ToString("yyyyMMddHHmmssfff"), Path.GetExtension(Files));

                    string fileLocation = LegacyWeb.MapPath("~/uploads/excelitem/") + Files;//Request.Form.Files["file"].FileName;

                    string fileLoc = LegacyWeb.MapPath("~/uploads/excelitem/");
                    if (!Directory.Exists(fileLoc))
                        Directory.CreateDirectory(fileLoc);

                    Request.Form.Files["file"].SaveAs(fileLocation);
                    string excelConnectionString = string.Empty;
                    excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                    fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    //connection String for xls file format.
                    if (fileExtension == ".xls")
                    {
                        excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
                        fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                    }
                    //connection String for xlsx file format.
                    else if (fileExtension == ".xlsx")
                    {
                        excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                        fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    }
                    //Create Connection to Excel work book and add oledb namespace
                    OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                    excelConnection.Open();
                    DataTable dt = new DataTable();

                    dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    if (dt == null)
                    {
                        return null;
                    }

                    String[] excelSheets = new String[dt.Rows.Count];
                    int t = 0;
                    //excel data saves in temp file here.
                    foreach (DataRow row in dt.Rows)
                    {
                        excelSheets[t] = row["TABLE_NAME"].ToString();
                        t++;
                    }
                    OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);


                    string query = string.Format("Select * from [{0}]", excelSheets[0]);
                    using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                    {
                        dataAdapter.Fill(ds);
                    }

                    //upload items
                    if (selecttype == "Item")
                    {
                        ImportItem(ds);
                    }
                    if (selecttype == "Itemwithsupplier")
                    {
                        ImportItemwithsupplier(ds);
                    }
                    //upload customer
                    if (selecttype == "Customer")
                    {
                        ImportCustomer(ds);
                    }
                    //upload supplier
                    if (selecttype == "Supplier")
                    {
                        ImportSupplier(ds);
                    }
                    if (selecttype == "Accounts")
                    {
                        ImportAccount(ds);
                    }

                    return RedirectToAction("Import", "ImportExcel");
                }
                else
                {
                    Danger("Please Upload Excel file..", false);
                    return RedirectToAction("Import", "ImportExcel");
                }
            }
            else
            {
                Danger("Please Upload file..", false);
                return RedirectToAction("Import", "ImportExcel");
            }
        }
        #region itemwithsupplier
        public void ImportItemwithsupplier(DataSet ds)
        {

            DataTable dtCol = new DataTable();
            dtCol.Columns.Add("Sl No");
            dtCol.Columns.Add("Item Code");
            dtCol.Columns.Add("Item Name");
            dtCol.Columns.Add("Arabic Name");
            dtCol.Columns.Add("Barcode");
            dtCol.Columns.Add("Category");
            dtCol.Columns.Add("Selling Price");
            dtCol.Columns.Add("Purchase Price");
            dtCol.Columns.Add("Base Price");
            dtCol.Columns.Add("MRP");
            dtCol.Columns.Add("Brand");
            dtCol.Columns.Add("Size");
            dtCol.Columns.Add("Tax");
            dtCol.Columns.Add("Color");
            dtCol.Columns.Add("Primary Unit");
            dtCol.Columns.Add("Secondary Unit");
            dtCol.Columns.Add("Convertion Factor");
            dtCol.Columns.Add("Item Description");
            dtCol.Columns.Add("Opening Stock");
            dtCol.Columns.Add("Minimun Stock");

            Int32 chkCount = 0;
            DataTable newdt = ds.Tables[0];
            foreach (DataColumn dc in newdt.Columns)
            {
                if (chkHeader(dc.ColumnName, dtCol))
                {
                    chkCount++;
                }
            }
            if (1 == 1)
            {
                if (newdt.Rows.Count > 0)
                {
                    Int32 ItmCount = 0;
                    var enable = db.EnableSettings.Where(a => a.EnableType == "Barcode").FirstOrDefault();
                    var brcheck = enable != null ? enable.Status : Status.inactive;
                    var UserId = User.Identity.GetUserId();
                    for (int i = 0; i < newdt.Rows.Count; i++)
                    {
                        if (newdt.Rows[i]["Item Name"] != DBNull.Value)
                        {
                            Item Items = new Item();
                            //if itemcode null,then auto generated
                            Items.ItemCode = newdt.Rows[i]["Item Code"] == DBNull.Value ? ItemCodes() : newdt.Rows[i]["Item Code"].ToString();

                            Items.ItemName = newdt.Rows[i]["Item Name"].ToString().Replace("'", "''");
                            Items.ItemArabic = newdt.Rows[i]["Arabic Name"].ToString();

                            var barcode = newdt.Rows[i]["Barcode"].ToString();
                            Items.Barcode = "";
                            if (barcode != "")
                            {
                                var bcode = newdt.Rows[i]["Barcode"].ToString();
                                var barcodeExists = db.Barcodes.Any(c => c.BarcodeNumber == bcode);
                                if (!barcodeExists)
                                {
                                    Items.Barcode = brcheck == Status.active ? barcode : "";
                                }
                            }


                            Items.ItemCategoryID = newdt.Rows[i]["Category"] == DBNull.Value ? 1 : getCategoryId(newdt.Rows[i]["Category"].ToString());
                            long supplierid = addsupplier(newdt.Rows[i]["supplier Code"].ToString(), newdt.Rows[i]["Supplier Name"].ToString());
                            Items.SellingPrice = newdt.Rows[i]["Selling Price"] != DBNull.Value ? Convert.ToDecimal(newdt.Rows[i]["Selling Price"]) : 0;
                            Items.PurchasePrice = newdt.Rows[i]["Purchase Price"] != DBNull.Value ? Convert.ToDecimal(newdt.Rows[i]["Purchase Price"]) : 0;
                            Items.BasePrice = newdt.Rows[i]["Base Price"] != DBNull.Value ? Convert.ToDecimal(newdt.Rows[i]["Base Price"]) : 0;
                            Items.MRP = newdt.Rows[i]["MRP"] != DBNull.Value ? Convert.ToDecimal(newdt.Rows[i]["MRP"]) : 0;

                            Items.ItemBrandID = newdt.Rows[i]["Brand"] == DBNull.Value ? 1 : getBrandId(newdt.Rows[i]["Brand"].ToString());
                            Items.ItemSizeID = newdt.Rows[i]["Size"] == DBNull.Value ? (long?)null : getSizeId(newdt.Rows[i]["Size"].ToString());

                            Items.TaxID =2;

                            Items.ItemColorID = newdt.Rows[i]["Color"] == DBNull.Value ? (long?)null : getColorId(newdt.Rows[i]["Color"].ToString());

                            Items.ItemUnitID = newdt.Rows[i]["Primary Unit"] == DBNull.Value ? (long?)null : getUnitId(newdt.Rows[i]["Primary Unit"].ToString());

                            Items.SubUnitId = newdt.Rows[i]["Secondary Unit"] == DBNull.Value ? (long?)null : getUnitId(newdt.Rows[i]["Secondary Unit"].ToString());
                            //cofactor checking
                            decimal conFactor = 1;
                            if ((newdt.Rows[i]["Primary Unit"] != DBNull.Value) && (newdt.Rows[i]["Secondary Unit"] != DBNull.Value))
                            {
                                if (newdt.Rows[i]["Primary Unit"] == newdt.Rows[i]["Secondary Unit"])
                                {
                                    conFactor = 1;
                                }
                                else
                                {
                                    if (newdt.Rows[i]["Convertion Factor"] != DBNull.Value)
                                        conFactor = Convert.ToDecimal(newdt.Rows[i]["Convertion Factor"]);
                                    else
                                        conFactor = 1;
                                }
                            }

                            Items.ConFactor = newdt.Rows[i]["Convertion Factor"] == DBNull.Value ? 1 : conFactor;
                            Items.ItemDescription = newdt.Rows[i]["Item Description"].ToString().Replace("'", "''");
                            Items.OpeningStock = newdt.Rows[i]["Opening Stock"] != DBNull.Value ? Convert.ToDecimal(newdt.Rows[i]["Opening Stock"]) : 0;
                            Items.MinStock = newdt.Rows[i]["Minimun Stock"] != DBNull.Value ? Convert.ToDecimal(newdt.Rows[i]["Minimun Stock"]) : 0;

                            Items.Status = Status.active;
                            if (supplierid != 0)
                                Items.Supplier = supplierid;
                            Items.CreatedUserID = User.Identity.GetUserId();
                            Items.CreatedBy = 1;
                            Items.ItemType = 1;
                            Items.KeepStock = Items.OpeningStock != null ? (Items.OpeningStock > 0 ? true : false) : false;


                            db.Items.Add(Items);
                            db.SaveChanges();
                            com.addlog(LogTypes.Created, UserId, "Item", "Items", findip(), Items.ItemID, "Item Added Successfully");

                            //save barcode
                            if (barcode != "")
                            {
                                var bcode = newdt.Rows[i]["Barcode"].ToString();
                                var barcodeExists = db.Barcodes.Any(c => c.BarcodeNumber == bcode);
                                if (!barcodeExists)
                                {
                                    var brcode = new Barcode
                                    {
                                        BarcodeNumber = barcode,
                                        ItemID = Items.ItemID
                                    };
                                    db.Barcodes.Add(brcode);
                                    db.SaveChanges();
                                }
                            }
                            ItmCount++;
                        }
                    }
                    Success(ItmCount + " Items Uploaded.", true);
                }
                else
                {
                    Warning("Excel is Empty..", false);
                }
            }
            else
            {
                Danger("Please Check Excel Format....", false);
            }

        }

        #endregion
        #region item
        public void ImportItem(DataSet ds)
        {

            DataTable dtCol = new DataTable();
            dtCol.Columns.Add("Sl No");
            dtCol.Columns.Add("Item Code");
            dtCol.Columns.Add("Item Name");
            dtCol.Columns.Add("Arabic Name");
            dtCol.Columns.Add("Barcode");
            dtCol.Columns.Add("Category");
            dtCol.Columns.Add("Selling Price");
            dtCol.Columns.Add("Purchase Price");
            dtCol.Columns.Add("Base Price");
            dtCol.Columns.Add("MRP");
            dtCol.Columns.Add("Brand");
            dtCol.Columns.Add("Size");
            dtCol.Columns.Add("Tax");
            dtCol.Columns.Add("Color");
            dtCol.Columns.Add("Primary Unit");
            dtCol.Columns.Add("Secondary Unit");
            dtCol.Columns.Add("Convertion Factor");
            dtCol.Columns.Add("Item Description");
            dtCol.Columns.Add("Opening Stock");
            dtCol.Columns.Add("Minimun Stock");

            Int32 chkCount = 0;
            DataTable newdt = ds.Tables[0];
            foreach (DataColumn dc in newdt.Columns)
            {
                if (chkHeader(dc.ColumnName, dtCol))
                {
                    chkCount++;
                }
            }
            if (1==1)
            {
                if (newdt.Rows.Count > 0)
                {
                    Int32 ItmCount = 0;
                    var enable = db.EnableSettings.Where(a => a.EnableType == "Barcode").FirstOrDefault();
                    var brcheck = enable != null ? enable.Status : Status.inactive;
                    var UserId = User.Identity.GetUserId();
                    for (int i = 0; i < newdt.Rows.Count; i++)
                    {
                        if (newdt.Rows[i]["Item Name"] != DBNull.Value)
                        {
                            Item Items = new Item();
                            //if itemcode null,then auto generated
                            Items.ItemCode = newdt.Rows[i]["Item Code"] == DBNull.Value ? ItemCodes() : newdt.Rows[i]["Item Code"].ToString();

                            Items.ItemName = newdt.Rows[i]["Item Name"].ToString().Replace("'", "''");
                            Items.ItemArabic = newdt.Rows[i]["Arabic Name"].ToString();

                            var barcode = newdt.Rows[i]["Barcode"].ToString();
                            Items.Barcode = "";
                            if (barcode != "")
                            {
                                var bcode = newdt.Rows[i]["Barcode"].ToString();
                                var barcodeExists = db.Barcodes.Any(c => c.BarcodeNumber == bcode);
                                if (!barcodeExists)
                                {
                                    Items.Barcode = brcheck == Status.active ? barcode : "";
                                }
                            }


                            Items.ItemCategoryID = newdt.Rows[i]["Category"] == DBNull.Value ? 1 : getCategoryId(newdt.Rows[i]["Category"].ToString());

                            Items.SellingPrice = newdt.Rows[i]["Selling Price"] != DBNull.Value ? Convert.ToDecimal(newdt.Rows[i]["Selling Price"]) : 0;
                            Items.PurchasePrice = newdt.Rows[i]["Purchase Price"] != DBNull.Value ? Convert.ToDecimal(newdt.Rows[i]["Purchase Price"]) : 0;
                            Items.BasePrice = newdt.Rows[i]["Base Price"] != DBNull.Value ? Convert.ToDecimal(newdt.Rows[i]["Base Price"]) : 0;
                            Items.MRP = newdt.Rows[i]["MRP"] != DBNull.Value ? Convert.ToDecimal(newdt.Rows[i]["MRP"]) : 0;

                            Items.ItemBrandID = newdt.Rows[i]["Brand"] == DBNull.Value ? 1 : getBrandId(newdt.Rows[i]["Brand"].ToString());
                            Items.ItemSizeID = newdt.Rows[i]["Size"] == DBNull.Value ? (long?)null : getSizeId(newdt.Rows[i]["Size"].ToString());

                            Items.TaxID = 2;// newdt.Rows[i]["Tax"] == DBNull.Value ? 2 : getTaxId(newdt.Rows[i]["Tax"].ToString());

                            Items.ItemColorID = newdt.Rows[i]["Color"] == DBNull.Value ? (long?)null : getColorId(newdt.Rows[i]["Color"].ToString());

                            Items.ItemUnitID = newdt.Rows[i]["Primary Unit"] == DBNull.Value ? (long?)null : getUnitId(newdt.Rows[i]["Primary Unit"].ToString());

                            Items.SubUnitId = newdt.Rows[i]["Secondary Unit"] == DBNull.Value ? (long?)null : getUnitId(newdt.Rows[i]["Secondary Unit"].ToString());
                            //cofactor checking
                            decimal conFactor = 1;
                            if ((newdt.Rows[i]["Primary Unit"] != DBNull.Value) && (newdt.Rows[i]["Secondary Unit"] != DBNull.Value))
                            {
                                if (newdt.Rows[i]["Primary Unit"] == newdt.Rows[i]["Secondary Unit"])
                                {
                                    conFactor = 1;
                                }
                                else
                                {
                                    conFactor = Convert.ToDecimal(newdt.Rows[i]["Convertion Factor"]);
                                }
                            }

                            Items.ConFactor = newdt.Rows[i]["Convertion Factor"] == DBNull.Value ? 1 : conFactor;
                            Items.ItemDescription = newdt.Rows[i]["Item Description"].ToString().Replace("'", "''");
                            Items.OpeningStock = newdt.Rows[i]["Opening Stock"] != DBNull.Value ? Convert.ToDecimal(newdt.Rows[i]["Opening Stock"]) : 0;
                                   Items.MinStock = newdt.Rows[i]["Minimun Stock"] != DBNull.Value ? Convert.ToDecimal(newdt.Rows[i]["Minimun Stock"]) : 0;

                            Items.Status = Status.active;
                            Items.CreatedUserID = User.Identity.GetUserId();
                            Items.CreatedBy = 1;
                            Items.ItemType = 1;
                            Items.KeepStock = Items.OpeningStock != null ? (Items.OpeningStock > 0 ? true : false) : false;


                            db.Items.Add(Items);
                            db.SaveChanges();
                            com.addlog(LogTypes.Created, UserId, "Item", "Items", findip(), Items.ItemID, "Item Added Successfully");

                            //save barcode
                            if (barcode != "")
                            {
                                var bcode = newdt.Rows[i]["Barcode"].ToString();
                                var barcodeExists = db.Barcodes.Any(c => c.BarcodeNumber == bcode);
                                if (!barcodeExists)
                                {
                                    var brcode = new Barcode
                                    {
                                        BarcodeNumber = barcode,
                                        ItemID = Items.ItemID
                                    };
                                    db.Barcodes.Add(brcode);
                                    db.SaveChanges();
                                }
                            }
                            ItmCount++;
                        }
                    }
                    Success(ItmCount + " Items Uploaded.", true);
                }
                else
                {
                    Warning("Excel is Empty..", false);
                }
            }
            else
            {
                Danger("Please Check Excel Format....", false);
            }

        }





        private string ItemCodes(Int64 INo = 0, string ICode = null)
        {
            var prefix = db.CodePrefixs.Where(a => a.section == "Item").Select(a => a.prefix).FirstOrDefault();
            if (ICode == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == "Item").Select(a => a.number).FirstOrDefault();
                if ((db.Items.Select(p => p.ItemID).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    if (number == 0)
                    {
                        ICode = prefix + 1;
                    }
                    else
                    {
                        ICode = prefix + number;
                    }
                }
                else
                {
                    INo = db.Items.Max(p => p.ItemID + 1);
                    ICode = prefix + INo;
                    if (CodeExist(ICode))
                    {
                        ICode = ItemCodes(INo, ICode);
                    }

                }
            }
            else
            {
                INo = INo + 1;
                ICode = prefix + INo;
                if (CodeExist(ICode))
                {
                    ICode = ItemCodes(INo, ICode);
                }
            }
            return ICode;
        }
        private bool CodeExist(string Code)
        {
            var Exists = db.Items.Any(c => c.ItemCode == Code);
            bool res = (Exists) ? true : false;
            return res;
        }

        private bool chkHeader(string header, DataTable dtcol)
        {
            DataColumnCollection columns = dtcol.Columns;
            bool check = true;
            if (!columns.Contains(header))
            {
                check = false;
            }
            return check;
        }
        //get categoryId
        private Int64 getCategoryId(string category)
        {
            Int64 catID = 0;
            var Exists = db.ItemCategorys.Any(c => c.ItemCategoryName == category);
            if (Exists)
            {
                catID = db.ItemCategorys.Where(c => c.ItemCategoryName == category).Select(c => c.ItemCategoryID).FirstOrDefault();
            }
            else
            {
                ItemCategory catgy = new ItemCategory();
                catgy.ItemCategoryName = category;
                catgy.Editable = choice.Yes;
                db.ItemCategorys.Add(catgy);
                db.SaveChanges();
                catID = catgy.ItemCategoryID;
            }
            return catID;
        }

        //get brandId
        private Int64 getBrandId(string brand)
        {
            Int64 brID = 0;
            var Exists = db.ItemBrands.Any(c => c.ItemBrandName == brand);
            if (Exists)
            {
                brID = db.ItemBrands.Where(c => c.ItemBrandName == brand).Select(c => c.ItemBrandID).FirstOrDefault();
            }
            else
            {
                ItemBrand brd = new ItemBrand();
                brd.ItemBrandName = brand;
                db.ItemBrands.Add(brd);
                db.SaveChanges();
                brID = brd.ItemBrandID;
            }
            return brID;
        }
        //get TaxId
        private Int64 getTaxId(string tax)
        {
            Int64 txID = 0;
            var Exists = db.Taxs.Any(c => c.TaxName == tax);
            if (Exists)
            {
                txID = db.Taxs.Where(c => c.TaxName == tax).Select(c => c.TaxID).FirstOrDefault();
            }
            else
            {
                Tax tx = new Tax();
                tx.TaxName = tax;
                db.Taxs.Add(tx);
                db.SaveChanges();
                txID = tx.TaxID;
            }
            return txID;
        }
        //get size id
        private Int64 getSizeId(string size)
        {
            Int64 sizeID = 0;
            var Exists = db.ItemSizes.Any(c => c.ItemSizeName == size);
            if (Exists)
            {
                sizeID = db.ItemSizes.Where(c => c.ItemSizeName == size).Select(c => c.ItemSizeID).FirstOrDefault();
            }
            else
            {
                ItemSize sizes = new ItemSize();
                sizes.ItemSizeName = size;
                db.ItemSizes.Add(sizes);
                db.SaveChanges();
                sizeID = sizes.ItemSizeID;
            }
            return sizeID;
        }

        //get color id
        private Int64 getColorId(string colors)
        {
            Int64 colorID = 0;
            var Exists = db.ItemColors.Any(c => c.ItemColorName == colors);
            if (Exists)
            {
                colorID = db.ItemColors.Where(c => c.ItemColorName == colors).Select(c => c.ItemColorID).FirstOrDefault();
            }
            else
            {
                ItemColor color = new ItemColor();
                color.ItemColorName = colors;
                db.ItemColors.Add(color);
                db.SaveChanges();
                colorID = color.ItemColorID;
            }
            return colorID;
        }
        //get UnitId
        private Int64 getUnitId(string unit)
        {
            Int64 unitID = 0;
            var Exists = db.ItemUnits.Any(c => c.ItemUnitName == unit);
            if (Exists)
            {
                unitID = db.ItemUnits.Where(c => c.ItemUnitName == unit).Select(c => c.ItemUnitID).FirstOrDefault();
            }
            else
            {
                ItemUnit units = new ItemUnit();
                units.ItemUnitName = unit;
                units.Editable = choice.Yes;
                db.ItemUnits.Add(units);
                db.SaveChanges();
                unitID = units.ItemUnitID;
            }
            return unitID;
        }


        #endregion

        #region customer
        public void ImportCustomer(DataSet ds)
        {
       
  DataTable dtCol = new DataTable();
            dtCol.Columns.Add("Sl No");
            dtCol.Columns.Add("Customer Code");
            dtCol.Columns.Add("Customer Name");
            dtCol.Columns.Add("Tax RegNo");
            dtCol.Columns.Add("Credit Limit");
            dtCol.Columns.Add("Credit Period");
            dtCol.Columns.Add("Location");
            dtCol.Columns.Add("Remark");
            dtCol.Columns.Add("Address");
            dtCol.Columns.Add("City");
            dtCol.Columns.Add("Emirate");
            dtCol.Columns.Add("Zip");
            dtCol.Columns.Add("Phone");
            dtCol.Columns.Add("Mobile");
           
            dtCol.Columns.Add("Fax");
            dtCol.Columns.Add("Email Id");
            dtCol.Columns.Add("Reference");
            dtCol.Columns.Add("Contact Person");
            dtCol.Columns.Add("Bank Name");
            dtCol.Columns.Add("Account No");
            dtCol.Columns.Add("Iban No");
            dtCol.Columns.Add("Branch Name");
            dtCol.Columns.Add("Swift");
            dtCol.Columns.Add("Opening Balance");
            dtCol.Columns.Add("Debit/Credit");


            Int32 chkCount = 0;
            DataTable newdt = ds.Tables[0];
            foreach (DataColumn dc in newdt.Columns)
            {
                if (chkHeader(dc.ColumnName, dtCol))
                {
                    chkCount++;
                }
            }
            if (chkCount == dtCol.Columns.Count-1)
            {
                if (newdt.Rows.Count > 0)
                {
                    Int32 ItmCount = 0;
                    var UserId = User.Identity.GetUserId();
                    for (int i = 0; i < newdt.Rows.Count; i++)
                    {
                        if (newdt.Rows[i]["Customer Name"] != DBNull.Value)
                        {
                            Int64 contactId = 0;
                            Int64 accountId = 0;

                            Contact contact = new Contact();

                            contact.Name = newdt.Rows[i]["Customer Name"].ToString().Replace("'", "''");
                            contact.Address = newdt.Rows[i]["Address"].ToString().Replace("'", "''");
                            contact.City = newdt.Rows[i]["City"].ToString().Replace("'", "''");
                            contact.State = newdt.Rows[i]["Emirate"] == DBNull.Value ? "Abu Dhabi" : newdt.Rows[i]["Emirate"].ToString();
                            contact.Country = "UAE";
                            contact.Zip = newdt.Rows[i]["Zip"].ToString().Replace("'", "''");
                            contact.Phone = newdt.Rows[i]["Phone"] == DBNull.Value ? null : newdt.Rows[i]["Phone"].ToString();
                            contact.Fax = newdt.Rows[i]["Fax"] == DBNull.Value ? null : newdt.Rows[i]["Fax"].ToString();
                            contact.EmailId = newdt.Rows[i]["Email Id"] == DBNull.Value ? null : newdt.Rows[i]["Email Id"].ToString();
                            contact.Reference = newdt.Rows[i]["Reference"].ToString().Replace("'", "''");
                            contact.ContactPerson = newdt.Rows[i]["Contact Person"].ToString().Replace("'", "''");
                            contact.Group = 2;
                            contact.Status = Status.active;


                            db.Contacts.Add(contact);
                            db.SaveChanges();
                            Int64 ContactId = contact.ContactID;
                            Mobile mob = new Mobile();
                            mob.Contact = ContactId;
                            mob.MobileNum= newdt.Rows[i]["Mobile"] == DBNull.Value ? null : newdt.Rows[i]["Mobile"].ToString();
                           
                            db.Mobiles.Add(mob);
                            db.SaveChanges();

                          

                            ContactRelation Relation = new ContactRelation();
                            Relation.ContactID = contactId;
                            Relation.RelationType = (int)ContctRelation.Customer;//for customer
                            Relation.RelationID = ContactId;
                            db.ContactRelation.Add(Relation);
                            db.SaveChanges();

                            decimal openbal = newdt.Rows[i]["Opening Balance"] != DBNull.Value ? Convert.ToDecimal(newdt.Rows[i]["Opening Balance"]) : 0;
                            string DorC = newdt.Rows[i]["Debit/Credit"].ToString();

                            contactId = contact.ContactID;

                            Accounts account = new Accounts();
                            account.Name = newdt.Rows[i]["Customer Name"].ToString().Replace("'", "''");
                            account.Alias = newdt.Rows[i]["Customer Name"].ToString().Replace("'", "''");
                            account.PrintName = newdt.Rows[i]["Customer Name"].ToString().Replace("'", "''");
                            account.Group = 12;
                            account.Status = Status.active;
                            account.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                            account.TRN = newdt.Rows[i]["Tax RegNo"].ToString().Replace("'", "''");
                            if (DorC == "Debit")
                            {
                                account.OpnBalance = openbal;
                                account.OpnBalanceCr = 0;
                            }
                            if (DorC == "Credit")
                            {
                                account.OpnBalance = 0;
                                account.OpnBalanceCr = openbal;
                            }

                            db.Accountss.Add(account);
                            db.SaveChanges();
                            accountId = account.AccountsID;




                            Customer cust = new Customer();
                            cust.Contact = contactId;
                            cust.Accounts = accountId;

                            cust.CustomerName = newdt.Rows[i]["Customer Name"].ToString().Replace("'", "''");
                            cust.TaxRegNo = newdt.Rows[i]["Tax RegNo"] == DBNull.Value ? CustCode() : newdt.Rows[i]["Tax RegNo"].ToString(); ;
                            cust.CreditLimit = newdt.Rows[i]["Credit Limit"] != DBNull.Value ? Convert.ToDecimal(newdt.Rows[i]["Credit Limit"]) : 0;
                            cust.CreditPeriod = newdt.Rows[i]["Credit Period"] != DBNull.Value ? Convert.ToInt32(newdt.Rows[i]["Credit Period"]) : 0;

                            cust.SalesPerson = null;

                            cust.Location = newdt.Rows[i]["Location"].ToString().Replace("'", "''");
                            cust.Remark = newdt.Rows[i]["Remark"].ToString().Replace("'", "''");
                            cust.BankName = newdt.Rows[i]["Bank Name"].ToString().Replace("'", "''");
                            cust.AccountNo = newdt.Rows[i]["Account No"].ToString().Replace("'", "''");
                            cust.IbanNo = newdt.Rows[i]["Iban No"].ToString().Replace("'", "''");
                            cust.BranchName = newdt.Rows[i]["Branch Name"].ToString().Replace("'", "''");
                            cust.Swift = newdt.Rows[i]["Swift"].ToString().Replace("'", "''");


                            db.Customers.Add(cust);
                            db.SaveChanges();

                            if (openbal > 0)
                            {
                                if (DorC == "Debit")
                                {
                                    com.addAccountTrasaction(openbal, 0, accountId, "Opening Balance", accountId, DC.Debit);

                                }
                                if (DorC == "Credit")
                                {
                                    com.addAccountTrasaction(0, openbal, accountId, "Opening Balance", accountId, DC.Credit);
                                }
                            }



                            com.addlog(LogTypes.Created, UserId, "Customer", "Customers", findip(), cust.CustomerID, "Customer Added Successfully");

                            ItmCount++;
                        }
                    }
                    Success(ItmCount + " Customer Uploaded.", true);
                }
                else
                {
                    Warning("Excel is Empty..", false);
                }
            }
            else
            {
                Danger("Please Check Excel Format....", false);
            }
        }



        private string CustCode(Int64 CNo = 0, string CCode = null)
        {
            var prefix = db.CodePrefixs.Where(a => a.section == "Customer").Select(a => a.prefix).FirstOrDefault();

            if (CCode == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == "Customer").Select(a => a.number).FirstOrDefault();
                if ((db.Customers.Select(p => p.CustomerID).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    if (number == 0)
                    {
                        CCode = prefix + 1;
                    }
                    else
                    {
                        CCode = prefix + number;
                    }
                }
                else
                {
                    CNo = db.Customers.Max(p => p.CustomerID + 1);
                    CCode = prefix + CNo;
                    if (CustCodeExist(CCode))
                    {
                        CCode = CustCode(CNo, CCode);
                    }

                }
            }
            else
            {
                CNo = CNo + 1;
                CCode = prefix + CNo;
                if (CustCodeExist(CCode))
                {
                    CCode = CustCode(CNo, CCode);
                }
            }
            return CCode;
        }
        private bool CustCodeExist(string Code)
        {
            var Exists = db.Customers.Any(c => c.CustomerCode == Code);
            bool res = (Exists) ? true : false;
            return res;
        }




        //get categoryId



        #endregion


        #region supplier
        public void ImportSupplier(DataSet ds)
        {
            DataTable dtCol = new DataTable();
            dtCol.Columns.Add("Sl No");
            dtCol.Columns.Add("Supplier Code");
            dtCol.Columns.Add("Supplier Name");
            dtCol.Columns.Add("Tax RegNo");
            dtCol.Columns.Add("Credit Limit");
            dtCol.Columns.Add("Credit Period");
            dtCol.Columns.Add("Remark");
            dtCol.Columns.Add("Address");
            dtCol.Columns.Add("City");
            dtCol.Columns.Add("Emirate");
            dtCol.Columns.Add("Zip");
            dtCol.Columns.Add("Phone");
            dtCol.Columns.Add("Mobile");
            dtCol.Columns.Add("Fax");
            dtCol.Columns.Add("Email Id");
            dtCol.Columns.Add("Reference");
            dtCol.Columns.Add("Sales Person");
            dtCol.Columns.Add("Sales Person Mobile");
            dtCol.Columns.Add("Bank Name");
            dtCol.Columns.Add("Account No");
            dtCol.Columns.Add("Iban No");
            dtCol.Columns.Add("Branch Name");
            dtCol.Columns.Add("Swift");
            dtCol.Columns.Add("Opening Balance");
            dtCol.Columns.Add("Debit/Credit");


            Int32 chkCount = 0;
            DataTable newdt = ds.Tables[0];
            foreach (DataColumn dc in newdt.Columns)
            {
                if (chkHeader(dc.ColumnName, dtCol))
                {
                    chkCount++;
                }
            }
            if (chkCount == dtCol.Columns.Count)
            {
                if (newdt.Rows.Count > 0)
                {
                    Int32 ItmCount = 0;
                    var UserId = User.Identity.GetUserId();
                    for (int i = 0; i < newdt.Rows.Count; i++)
                    {
                        if (newdt.Rows[i]["Supplier Name"] != DBNull.Value)
                        {
                            Int64 contactId = 0;
                            Int64 accountId = 0;

                            Contact contact = new Contact();

                            contact.Name = newdt.Rows[i]["Supplier Name"].ToString().Replace("'", "''");
                            contact.Address = newdt.Rows[i]["Address"].ToString().Replace("'", "''");
                            contact.City = newdt.Rows[i]["City"].ToString().Replace("'", "''");
                            contact.State = newdt.Rows[i]["Emirate"] == DBNull.Value ? "Abu Dhabi" : newdt.Rows[i]["Emirate"].ToString();
                            contact.Country = "UAE";
                            contact.Zip = newdt.Rows[i]["Zip"].ToString().Replace("'", "''");
                            contact.Phone = newdt.Rows[i]["Phone"] == DBNull.Value ? null : newdt.Rows[i]["Phone"].ToString();
                            contact.Fax = newdt.Rows[i]["Fax"] == DBNull.Value ? null : newdt.Rows[i]["Fax"].ToString();
                            contact.EmailId = newdt.Rows[i]["Email Id"] == DBNull.Value ? null : newdt.Rows[i]["Email Id"].ToString();
                            contact.Reference = newdt.Rows[i]["Reference"].ToString().Replace("'", "''");
                            contact.ContactPerson = newdt.Rows[i]["Sales Person"].ToString().Replace("'", "''");
                            contact.SalesPMob = newdt.Rows[i]["Sales Person Mobile"].ToString().Replace("'", "''");
                            contact.Group = 3;
                            contact.Status = Status.active;


                            db.Contacts.Add(contact);
                            db.SaveChanges();

                            contactId = contact.ContactID;

                            Mobile mob = new Mobile();
                            mob.Contact = contactId;
                            mob.MobileNum= newdt.Rows[i]["Mobile"] == DBNull.Value ? null : newdt.Rows[i]["Mobile"].ToString();
                            db.Mobiles.Add(mob);
                            db.SaveChanges();

                            decimal openbal = newdt.Rows[i]["Opening Balance"] != DBNull.Value ? Convert.ToDecimal(newdt.Rows[i]["Opening Balance"]) : 0;
                            string DorC = newdt.Rows[i]["Debit/Credit"].ToString();

                            Accounts account = new Accounts();
                            account.Name = newdt.Rows[i]["Supplier Name"].ToString().Replace("'", "''");
                            account.Alias = newdt.Rows[i]["Supplier Name"].ToString().Replace("'", "''");
                            account.PrintName = newdt.Rows[i]["Supplier Name"].ToString().Replace("'", "''");
                            account.Group = 14;
                            account.Status = Status.active;
                            account.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                            account.TRN = newdt.Rows[i]["Tax RegNo"].ToString().Replace("'", "''");
                            if (DorC == "Debit")
                            {
                                account.OpnBalance = openbal;
                                account.OpnBalanceCr = 0;
                            }
                            if (DorC == "Credit")
                            {
                                account.OpnBalance = 0;
                                account.OpnBalanceCr = openbal;
                            }

                            db.Accountss.Add(account);
                            db.SaveChanges();
                            accountId = account.AccountsID;




                            Supplier supp = new Supplier();
                            supp.Contact = contactId;
                            supp.Accounts = accountId;

                            supp.SupplierCode = newdt.Rows[i]["Supplier Code"] == DBNull.Value ? SuppCode() : newdt.Rows[i]["Supplier Code"].ToString();
                            supp.SupplierName = newdt.Rows[i]["Supplier Name"].ToString().Replace("'", "''");
                            supp.CreditLimit = newdt.Rows[i]["Credit Limit"] != DBNull.Value ? Convert.ToDecimal(newdt.Rows[i]["Credit Limit"]) : 0;
                            supp.CreditPeriod = newdt.Rows[i]["Credit Period"] != DBNull.Value ? Convert.ToInt32(newdt.Rows[i]["Credit Period"]) : 0;

                            supp.Remark = newdt.Rows[i]["Remark"].ToString().Replace("'", "''");
                            supp.BankName = newdt.Rows[i]["Bank Name"].ToString().Replace("'", "''");
                            supp.AccountNo = newdt.Rows[i]["Account No"].ToString().Replace("'", "''");
                            supp.IbanNo = newdt.Rows[i]["Iban No"].ToString().Replace("'", "''");
                            supp.BranchName = newdt.Rows[i]["Branch Name"].ToString().Replace("'", "''");
                            supp.Swift = newdt.Rows[i]["Swift"].ToString().Replace("'", "''");


                            db.Suppliers.Add(supp);
                            db.SaveChanges();

                            if (openbal > 0)
                            {
                                if (DorC == "Debit")
                                {
                                    com.addAccountTrasaction(openbal, 0, accountId, "Opening Balance", accountId, DC.Debit);

                                }
                                if (DorC == "Credit")
                                {
                                    com.addAccountTrasaction(0, openbal, accountId, "Opening Balance", accountId, DC.Credit);
                                }
                            }

                            com.addlog(LogTypes.Created, UserId, "Supplier", "Suppliers", findip(), supp.SupplierID, "Supplier Added Successfully");

                            ItmCount++;
                        }
                    }
                    Success(ItmCount + " Supplier Uploaded.", true);
                }
                else
                {
                    Warning("Excel is Empty..", false);
                }
            }
            else
            {
                Danger("Please Check Headers..", false);
            }
        }

        private string SuppCode(Int64 SNo = 0, string SCode = null)
        {
            var prefix = db.CodePrefixs.Where(a => a.section == "Supplier").Select(a => a.prefix).FirstOrDefault();
            Int32 number = db.CodePrefixs.Where(a => a.section == "Supplier").Select(a => a.number).FirstOrDefault();
            if (SCode == null)
            {
                if ((db.Suppliers.Select(p => p.SupplierID).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    if (number == 0)
                    {
                        SCode = prefix + 1;
                    }
                    else
                    {
                        SCode = prefix + number;
                    }
                }
                else
                {
                    SNo = db.Suppliers.Max(p => p.SupplierID + 1);
                    SCode = prefix + SNo;
                    if (SuppCodeExist(SCode))
                    {
                        SCode = SuppCode(SNo, SCode);
                    }

                }
            }
            else
            {
                SNo = SNo + 1;
                SCode = prefix + SNo;
                if (SuppCodeExist(SCode))
                {
                    SCode = SuppCode(SNo, SCode);
                }
            }
            return SCode;
        }
        private bool SuppCodeExist(string Code)
        {
            var Exists = db.Suppliers.Any(c => c.SupplierCode == Code);
            bool res = (Exists) ? true : false;
            return res;
        }

        #endregion

        #region account
        public void ImportAccount(DataSet ds)
        {
            DataTable dtCol = new DataTable();
            dtCol.Columns.Add("Sl No");
            dtCol.Columns.Add("Name");
            dtCol.Columns.Add("Group");
            dtCol.Columns.Add("Opening Balance");
            dtCol.Columns.Add("Debit/Credit");
            dtCol.Columns.Add("Note");


            Int32 chkCount = 0;
            DataTable newdt = ds.Tables[0];
            foreach (DataColumn dc in newdt.Columns)
            {
                if (chkHeader(dc.ColumnName, dtCol))
                {
                    chkCount++;
                }
            }
            if (chkCount == dtCol.Columns.Count)
            {
                if (newdt.Rows.Count > 0)
                {
                    Int32 ItmCount = 0;
                    var UserId = User.Identity.GetUserId();
                    for (int i = 0; i < newdt.Rows.Count; i++)
                    {
                        if (newdt.Rows[i]["Name"] != DBNull.Value)
                        {
                            string accname = newdt.Rows[i]["Name"].ToString().Replace("'", "''");
                            string Group = newdt.Rows[i]["Group"].ToString().Replace("'", "''");

                            var Exists = db.Accountss.Any(c => c.Name == accname && c.Group == 13);
                            if (!Exists && (Group != "" && Group != null))
                            {
                                string dc = newdt.Rows[i]["Debit/Credit"].ToString().Replace("'", "''");
                                dc = dc == "" ? DC.Debit.ToString() : dc;
                                decimal OpnBalance = newdt.Rows[i]["Opening Balance"] != null ? Convert.ToDecimal(newdt.Rows[i]["Opening Balance"]) : 0;

                                Accounts Acc = new Accounts();
                                if (dc == "Debit")
                                {
                                    Acc.OpnBalance = OpnBalance;
                                    Acc.OpnBalanceCr = 0;
                                }
                                if (dc == "Credit")
                                {
                                    Acc.OpnBalance = 0;
                                    Acc.OpnBalanceCr = OpnBalance;
                                }

                                Acc.PrintName = accname;
                                Acc.Name = accname;
                                Acc.Alias = accname;
                                Acc.PrevBalance = 0;
                                Acc.Note = newdt.Rows[i]["Note"].ToString().Replace("'", "''");
                                Acc.Status = Status.active;
                                Acc.Group = getAccGroupId(Group);

                                Acc.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                Acc.Editable = 0;

                                db.Accountss.Add(Acc);
                                db.SaveChanges();

                                if (OpnBalance > 0)
                                {
                                    if (dc == "Debit")
                                    {
                                        com.addAccountTrasaction(OpnBalance, 0, Acc.AccountsID, "Opening Balance", Acc.AccountsID, DC.Debit);

                                    }
                                    if (dc == "Credit")
                                    {
                                        com.addAccountTrasaction(0, OpnBalance, Acc.AccountsID, "Opening Balance", Acc.AccountsID, DC.Credit);
                                    }
                                }

                                com.addlog(LogTypes.Created, UserId, "Master", "Accountss", findip(), Acc.AccountsID, "Account Created Successfully");
                                ItmCount++;
                            }
                        }
                    }
                    Success(ItmCount + " Accounts Uploaded.", true);
                }
                else
                {
                    Warning("Excel is Empty..", false);
                }
            }
            else
            {
                Danger("Please Check Excel Format....", false);
            }
        }


        private Int64 getAccGroupId(string group)
        {
            Int64 GpID = 0;
            var Exists = db.AccountsGroups.Any(c => c.Name == group);
            if (Exists)
            {
                GpID = db.AccountsGroups.Where(c => c.Name == group).Select(c => c.AccountsGroupID).FirstOrDefault();
            }
            else
            {
                AccountsGroup accgp = new AccountsGroup();
                accgp.Name = group;
                accgp.Alias = group;
                accgp.Parent = 0;
                accgp.Primary = 0;
                accgp.Status = Status.active;

                db.AccountsGroups.Add(accgp);
                db.SaveChanges();
                GpID = accgp.AccountsGroupID;
            }
            return GpID;
        }

        #endregion

        //download excel format
        [HttpGet]
        public virtual ActionResult DownloadItemExcel(string file)
        {
            string fullPath = Path.Combine(LegacyWeb.MapPath("~/uploads/excelitem/excelformat/ItemExcelFormat.xlsx"));
            string fileName = "ItemExcelFormat.xlsx";
            return File(fullPath, "application/vnd.ms-excel", fileName);
        }
        [HttpGet]
        public virtual ActionResult DownloadSupplierExcel(string file)
        {
            string fullPath = Path.Combine(LegacyWeb.MapPath("~/uploads/excelitem/excelformat/SupplierExcelFormat.xlsx"));
            string fileName = "SupplierExcelFormat.xlsx";
            return File(fullPath, "application/vnd.ms-excel", fileName);
        }
        [HttpGet]
        public virtual ActionResult DownloadCustomerExcel(string file)
        {
            string fullPath = Path.Combine(LegacyWeb.MapPath("~/uploads/excelitem/excelformat/CustomerExcelFormat.xlsx"));
            string fileName = "CustomerExcelFormat.xlsx";
            return File(fullPath, "application/vnd.ms-excel", fileName);
        }
        [HttpGet]
        public virtual ActionResult DownloadExAccExcel(string file)
        {
            string fullPath = Path.Combine(LegacyWeb.MapPath("~/uploads/excelitem/excelformat/ExAccExcelFormat.xlsx"));
            string fileName = "ExAccExcelFormat.xlsx";
            return File(fullPath, "application/vnd.ms-excel", fileName);
        }
    }
}
