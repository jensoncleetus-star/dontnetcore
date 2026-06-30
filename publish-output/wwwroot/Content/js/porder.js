var count = 1, type = '';
limits = 500;
//Add Row 
var pcount = 2;
function addrow(t, action, ItemUnit, ItemTax, ItemTotalAmount, ItemQuantity, Item, ItemCode, ItemName, ItemUnitPrice, ItemSubTotal, ItemWithCode, ItemTaxAmount, ItemDiscount, ItemNote, itemdata, ItemCategory, ItemBrand, PPRICE, MRP, SellPrice, BPrice, tag1, tag2, tag3, tag4, tag5, prefix, division, supplier) {

    if (count == limits) alert("You have reached the limit of adding " + count + " inputs");
    else {

        var Option = "";
        var OptionPro = "";
        var OptionTsk = "";
        var OptionMake = "";
        var optionunit = "";
        var required = "";
        var slno = $('#addinvoiceItem tr').length + 1;
        var a = "item_name" + count,
        tabindex = count * 5;
        var row = "<tr class='item_' id='item_" + count + "'>";
        var data = "";
        var price = 0;
        var baseprice = 0;
        var mrp = 0;
        var htdata = "";
        var itemnote = "";
        var notbtn = "";
        var divid = "item_name_" + Item;
        var PurchaseType = $("#PurchaseType").val() || "";
        var SalesType = $("#SalesType").val() || "";
        if (PurchaseType == 2 || SalesType == 2) {
            ItemTax = 0;
            ItemTaxAmount = 0;
        }
        tab1 = tabindex + 1;
        tab2 = tabindex + 2;
        tab3 = tabindex + 3;
        tab4 = tabindex + 4;
        tab5 = tabindex + 5;
        tab6 = tabindex + 6;
        tab7 = tabindex + 7;
        tab8 = tabindex + 8;
        tab9 = tabindex + 9;

        if (Item != null) {
            row = "<tr class='item_" + Item + "' id='item_" + count + "'>";
            Option = "<option value='" + Item + "'>" + ItemWithCode + "</option>";
        }


        if (itemdata != null && itemdata.ProjectId != null && itemdata.ProjectName != null) {
            OptionPro = "<option value='" + itemdata.ProjectId + "'>" + itemdata.ProjectName + "</option>";
        }
        if (itemdata != null && itemdata.TaskId != null && itemdata.TaskName != null) {
            OptionTsk = "<option value='" + itemdata.TaskId + "'>" + itemdata.TaskName + "</option>";
        }
        if (itemdata != null && itemdata.ItemMake != null && itemdata.ItemMakeName != null) {
            OptionMake = "<option value='" + itemdata.ItemMake + "'>" + itemdata.ItemMakeName + "</option>"
        }

        //if(type == "excelpurchase")//newly added
        //{
        //    //alert(count + "," + Item);
        //    row = "<tr class='item_" + pcount + "' id='item_" + pcount + "'>";
        //    Option = "<option value='" + pcount + "'>" + ItemWithCode + "</option>";
        //    pcount++;
        //}

        if (count == 1) {
            required = 'required="required"';
        }
        if (action != '') {
            type = action;
        }
        var inote = "";
        if (itemdata) {
            inote = itemdata.note;
        }
        itemnote = '<div id="modal-item-' + count + '" class="modal fade" role="dialog" aria-hidden="true"><div class="modal-dialog"><div class="modal-content">' +
            '<div class="form-group"><textarea name="itemnote" cols="40" rows="10" class="form-control itemnote" id="itemnote-' + count + '" maxlength="1000">' + inote + '</textarea></div>' +
            '<div class="form-group"><button class="btn btn-info" type="button" data-dismiss="modal">Save</button></div>' +
            '</div></div></div>';
        notbtn = "<button type='button' class='itnote btn btn-default btn-flat' data-toggle='modal' data-target='#modal-item-" + count + "'><i class='fa fa-1x fa-file-text-o'></i></button>";

        if (itemdata) {
            if (type == "purchase" || type == "preturn")
                price = itemdata.PurchasePrice;
            else
                price = itemdata.SellingPrice;
            baseprice = itemdata.BasePrice;
            mrp = itemdata.MRP;
            if (type == "sales") {
                htdata = "<div class='minstock_" + count + "'";
                if (itemdata.KeepStock == true) {
                    var qntmin = 0;
                    if (itemdata.ItemUnit == itemdata.ItemUnitID) {
                        qntmin = ItemQuantity * itemdata.ConFactor;
                    }
                    if (itemdata.ItemUnit == itemdata.SubUnitId) {
                        qntmin = ItemQuantity;
                    }
                    totalstock = itemdata.total + qntmin;
                    minstock = itemdata.MinStock * itemdata.ConFactor;
                    htdata += " data-keeps='yes' data-min='" + minstock + "' data-confactor='" + itemdata.ConFactor + "' data-stock='" + totalstock + "'>";
                }
                else {
                    htdata += " data-keeps='no' >";
                }
                if ($(".minstock_" + count).length) {
                    $(".minstock_" + count).remove();
                }
            }
        }
        var itemaddbtn = "<span class='input-group-btn'><a type='button' href='/Item/AddItem' class='modal-create-lg btn btn-success btn-flat'><i class='fa fa-1x fa-plus-circle'></i></a>" + notbtn + "</span>";
        var brandaddbtn = "<span class='input-group-btn'><a type='button' href='/ItemBrand/Create' class='btnbrandAdd btn btn-success btn-flat'><i class='fa fa-1x fa-plus-circle'></i></a></span>";

        var protask = "";

        //-----------------------item bundle -----------------------------------------

        //alert(itemdata.SellingPrice);   
        //var bunitem = "<input type='hidden' class='descr' />";
        //var exrow = "";
        //if (itemdata != null && itemdata.bundle != null) {
        //    var chk = true;
        //    var bunId = "";
        //    $.each(itemdata.bundle, function (i, item) {
        //        bunId += (chk == true) ? item.SEItemsId : " , " + item.SEItemsId;
        //        chk = false;
        //    });
        //    bunitem = "<input type='hidden' class='descr' value= '" + bunId + "' />";
        //}

        //else {
        //    desc = "<br/>[<span class='descr' data-name='Note'>" + data.Note + "</span>]";
        //}
        //----------------------------------------------------------------------------------

        ItemDiscount = ItemDiscount != null ? ItemDiscount : 0;
        ItemCategory = ItemCategory != null ? ItemCategory : 0;
        ItemBrand = ItemBrand != null ? ItemBrand : 0;
        PPRICE = PPRICE != null ? PPRICE : 0;
        MRP = MRP != null ? MRP : 0;
        SellPrice = SellPrice != null ? SellPrice : 0;
        BPrice = BPrice != null ? BPrice : 0;
        data = "<td class='text-center' id=" + divid + "> " + slno + " </td>" +
                "<td class='input-group input-group-sm'><select class='form-control item_name' " + required + " data-id='" + count + "' placeholder='Item Name' id='item_name_" + count + "'  data-msg-required='The Item field is required' onchange='GetItemdetails(this," + count + ",\"" + type + "\")'>" + Option + "</select> " + itemaddbtn + "</td>" +
                "<td style='width:100px;'><select class='form-control units unit_name_" + count + "' id='unit_name_" + count + "' data-id='" + count + "' id='unit_name' onchange='unitchange(this," + count + ",\"" + type + "\"); '></select></td>" +
                "<td> <input type='number' name='product_quantity[]' data-msg-min ='The Item Quantity must be Greater than Zero' onchange='quantity_change(" + count + ");' id='total_qntt_" + count + "' value='" + ItemQuantity + "'  class='total_qntt_" + count + " form-control text-right quty' placeholder='0' value='0' min='.01' tabindex='" + tab2 + "'/></td>" +
                "<td><input type='number' name='product_rate[]' " + required + " data-msg-required='The Item Rate field is required' onchange='rate_change(" + count + ",\"" + type + "\");' id='price_item_" + count + "' value='" + ItemUnitPrice + "' class='price_item_" + count + " form-control text-right totrate' placeholder='0.00' min='0' tabindex='" + tab3 + "'/><input type='hidden' data-value='" + price + "' value='" + baseprice + "' name='base_rate' id='base_rate_" + count + "'> </td> " +
                "<td><input type='number' name='sub_total[]' id='sub_total_" + count + "' class='sub_total_" + count + " form-control text-right subtotal' value='" + ItemSubTotal + "'   placeholder='0.00' min='0' tabindex='" + tab3 + "' readonly='readonly'/></td>" +
                "<td><input type='number' name='item_discount[]' id='item_discount" + count + "' onchange='itemdiscount_change(" + count + ");' class='item_discount" + count + " form-control text-right item_discount' value='" + ItemDiscount + "' value='0.00' placeholder='0.00' tabindex='" + tab3 + "'/></td>" +
                "<td class='vwise'><input type='text' id='tax_" + count + "' class='form-control text-right tax tax_" + count + "' tabindex='" + tab4 + "' readonly='readonly' /><input type='hidden' class='item_amount' name='item_amount' id='item_amount_" + count + "'/><input type='hidden' class='tot_tax' name='tot_tax' id='tot_tax_" + count + "'/><input type='hidden'  class='tax_percentage' value='" + ItemTax + "' name='tax_percentage' id='tax_percentage_" + count + "'/></td> " +
                "<td class='text-right'><input class='total_price total_price_" + count + " form-control text-right' type='text' name='total_price[]' value='" + ItemTotalAmount + "' id='total_price_" + count + "' value='0.00' readonly='readonly'/><input type='hidden' class='cfactor' name='cfactor' id='cfactor_" + count + "'/> " +
                "<input type='hidden' class='selitem_name_" + count + "' value='" + ItemName + "' name='selitem_name' id='selitem_name_" + count + "'/> " +
                "<input type='hidden' class='selitem_code_" + count + "' value='" + ItemCode + "' name='selitem_code' id='selitem_code_" + count + "'/> " +
                "<input type='hidden' class='selitem_category_" + count + "' value='" + ItemCategory + "' name='selitem_category' id='selitem_category_" + count + "'/> " +
                "<input type='hidden' class='selitem_brand_" + count + "' value='" + ItemBrand + "' name='selitem_brand' id='selitem_brand_" + count + "'/> " +
                "<input type='hidden' class='selitem_pprice_" + count + "' value='" + PPRICE + "' name='selitem_pprice' id='selitem_pprice_" + count + "'/> " +
                "<input type='hidden' class='selitem_pmrp_" + count + "' value='" + MRP + "' name='selitem_pmrp' id='selitem_pmrp_" + count + "'/> " +
                "<input type='hidden' class='selitem_psprice_" + count + "' value='" + SellPrice + "' name='selitem_psprice' id='selitem_psprice_" + count + "'/> " +
                "<input type='hidden' class='selitem_pbprice_" + count + "' value='" + BPrice + "' name='selitem_pbprice' id='selitem_pbprice_" + count + "'/> " +
                "<input type='hidden' class='tagline1_" + count + "' value='" + tag1 + "'  name='tagline1' id='tagline1_" + count + "'/> " +
                "<input type='hidden' class='tagline2_" + count + "' value='" + tag2 + "'  name='tagline2' id='tagline2_" + count + "'/> " +
                "<input type='hidden' class='tagline3_" + count + "' value='" + tag3 + "'  name='tagline3' id='tagline3_" + count + "'/> " +
                "<input type='hidden' class='tagline4_" + count + "' value='" + tag4 + "' name='tagline4' id='tagline4_" + count + "'/> " +
                "<input type='hidden' class='tagline5_" + count + "' value='" + tag5 + "' name='tagline5' id='tagline5_" + count + "'/> " +
                "<input type='hidden' class='prefix_" + count + "' value='" + prefix + "' name='prefix' id='prefix_" + count + "'/> " +
                "<input type='hidden' class='division_" + count + "' value='" + division + "' name='division' id='division_" + count + "'/> " +
                "<input type='hidden' class='supplier_" + count + "' value='" + supplier + "' name='supplier' id='supplier_" + count + "'/> " +
                "</td>";



        var prochk = $("#procheck").val();
        if (prochk == "active" || (prochk == "active" && action == "purchase")) {
            protask += "<td><select class='form-control project_name' name='project_name' data-id='" + count + "' placeholder='Project' id='project_name_" + count + "'  onchange='GetProjectChange(this," + count + ",\"" + type + "\")'>" + OptionPro + "</select></td>";
            protask += "<td><select class='form-control task_name' data-id='" + count + "' name='task_name' placeholder='Task' id='task_name_" + count + "'>" + OptionTsk + "</select></td>";
        }

        var checkbr1 = "";
        var checkbr2 = "";
        var makecheck = $("#makecheck").val();
        if (makecheck == "active") {
            checkbr1 = "<td  class='input-group input-group-sm'><select class='form-control brand_name' name='brand_name' data-id='" + count + "' placeholder='brand' id='brand_name_" + count + "'>" + OptionMake + "</select>" + brandaddbtn + "</td>";
        }
        else {
            checkbr2 = "<input type='hidden' class='hidebrand_name' value='0' name='brand_name' id='brand_name_" + count + "'/>";
        }

        //"" + setproject + settask + "";
        protask += "" + checkbr1 + "<td class='text-center'>" + checkbr2 + "<button tabindex='" + tab5 + "' style='text-align: right;' class='btn btn-danger' type='button' value='Delete' onclick='deleteRow(this)'><i class='fa fa-trash fa-1x'></i></button>" + itemnote + htdata + "</td>";

        row += data + protask + "</tr>";
        $('#' + t).append(row);
        $('#itemnote-' + count).wysihtml5();
        if (Item != null) {
            $("#total_qntt_" + count).attr('readonly', false);
            $("#item_discount" + count).attr('readonly', false);
            $("#price_item_" + count).attr('readonly', false);
        }
        // $('#item_ .item_name').focus();
        searchItem();
        if (itemdata) {
            rate_change(count, type, 'foredit');
            createUnitList(itemdata, count, action);
        }
        else
            rate_change(count, type);
        count++;
        setTabIndex();
        searchbrand();

        searchproject();
        searchtask(count);

        if (type == "purchase") {
            VatVoucherWisePurchaseOrder();
        }
    }
}

//item details
function GetItemdetails(selectObject, dataid, action) {
    $("#total_qntt_" + dataid).attr('readonly', false);
    $("#item_discount" + dataid).attr('readonly', false);
    $("#price_item_" + dataid).attr('readonly', false);

    if (selectObject.value) {
        var ItemId = selectObject.value;
        if (ItemId != null) {
            if ($(".item_" + ItemId).length > 0) {
                if ($(".item_" + ItemId).length < 10) {
                    if (confirm('Are you sure want to Add this item Again?')) {
                        itemUpdate(selectObject, dataid, action);
                    }
                    else {
                        $(selectObject).val(null).trigger('change');
                    }
                }
                else {
                    alert("You Cannot Add same Item More than 4 Times");
                    $(selectObject).val(null).trigger('change');
                }
            }
            else {
                itemUpdate(selectObject, dataid, action);
            }
        }
    }
}


// update item details
function itemUpdate(selectObject, dataid, action) {

    var mc = $("#ddlMC").val();
    var Stype = "";
    if (action == "sales" || action == "quot") {
        var ROnlyRate = $("#ROnlyRate").val();
        if (ROnlyRate == "active") {
            $("#price_item_" + dataid).attr('readonly', true);
        }

    }
    var Stype = $('#SaleType').val();
    var Hire = $('#ddlHType').val();


    var newUrl;
    if (mc != null && mc > 0 && Stype != 2) {
        newUrl = '/Item/GetItemMC';
    }
    else {
        newUrl = '/Item/GetItem';
    }

    $.ajax({
        url: newUrl,
        type: "GET",
        dataType: "JSON",
        data: { itemID: selectObject.value, mc: mc, Saltype: Stype, HireType: Hire },
        success: function (result) {
            createUnitList(result, dataid, action);
            if (action == "sales" || action == "quot") {
                $(".price_item_" + dataid).val(result.SellingPrice);
                $("#item_amount_" + dataid).val(result.SellingPrice);
                $("#base_rate_" + dataid).attr("data-value", result.SellingPrice);
            }
            if (action == "purchase" || action == "preturn") {
                $(".price_item_" + dataid).val(result.PurchasePrice);
                $("#item_amount_" + dataid).val(result.SellingPrice);
                $("#base_rate_" + dataid).attr("data-value", result.PurchasePrice);
            }
            $("#total_qntt_" + dataid).val(1);
            var SalesType = $("#SalesType").val() || "";
            var PurchaseType = $("#PurchaseType").val() || "";
            var TaxP = result.Tax;
            if (PurchaseType == 2 || SalesType == 2) {
                TaxP = 0;
            }
            $("#tax_percentage_" + dataid).val(TaxP);
            $("#base_rate_" + dataid).val(result.BasePrice);
            $("#cfactor_" + dataid).val(result.ConFactor);

            rowSubTotal(dataid);
            CalculatetblItemListSum();
            grandtotalcalculation();
            paidamountcalculation();
            Discounts();

            if ((action != "sales" && action != "preturn") || ((action == "sales" || action == "preturn") && result.KeepStock != true) || ((action == "sales" || action == "preturn") && result.KeepStock == true && result.total > 0)) {
                // append item unit list 

                $(selectObject).closest('tr').attr('class', "item_" + result.ItemID);
                if (action == "sales" || action == "preturn") {

                    minstockupdate(result, dataid);
                }
                if ($(".item_").length == 0) {
                    addrow('addinvoiceItem', '', '', '0.00', '0.00', '0');
                }
                $('.unit_name_' + dataid).focus();
            }
            else if ((result.KeepStock == true && result.CheckStock == 0 && result.total <= 0)) {

                var res = confirm("Are you Sure Want To Add Items In Less Stock ?");
                if (res == true) {
                    $(selectObject).closest('tr').attr('class', "item_" + result.ItemID);
                    if (action == "sales" || action == "preturn") {
                        minstockupdate(result, dataid);
                    }
                    if ($(".item_").length == 0) {
                        addrow('addinvoiceItem', '', '', '0.00', '0.00', '0');
                    }
                    $('.unit_name_' + dataid).focus();
                }
                else {
                    $("#item_name_" + dataid).val(null).trigger("change");
                    $("#unit_name_" + dataid).val(null).trigger("change");
                    $("#total_qntt_" + dataid).val(null).trigger("change");
                    $("#price_item_" + dataid).val(null).trigger("change");
                }
            }
            else {
                alert("This Item is Out of Stock!!!");
                var classname = $($("#total_qntt_" + dataid)).closest('tr').attr('class');
                if (classname != 'item_') {
                    $("." + classname + " .btn-danger").click();
                }
                else {
                    $("#item_name_" + dataid).val(null).trigger("change");
                    $("#unit_name_" + dataid).val(null).trigger("change");
                    $("#total_qntt_" + dataid).val(null).trigger("change");
                    $("#price_item_" + dataid).val(null).trigger("change");
                }
            }
        }
    });
}
//    else {
//        $.ajax({
//            url: '/Item/GetItem',
//            type: "GET",
//            dataType: "JSON",
//            data: { itemID: selectObject.value },
//            success: function (result) {

//                createUnitList(result, dataid, action);
//                if (action == "sales" || action == "quot") {
//                    $(".price_item_" + dataid).val(result.SellingPrice);
//                    $("#item_amount_" + dataid).val(result.SellingPrice);
//                    $("#base_rate_" + dataid).attr("data-value", result.SellingPrice);
//                }
//                if (action == "purchase") {
//                    $(".price_item_" + dataid).val(result.PurchasePrice);
//                    $("#item_amount_" + dataid).val(result.SellingPrice);
//                    $("#base_rate_" + dataid).attr("data-value", result.PurchasePrice);
//                }

//                $("#total_qntt_" + dataid).val(1);
//                var SalesType = $("#SalesType").val() || "";
//                var PurchaseType = $("#PurchaseType").val() || "";
//                var TaxP = result.Tax;
//                if (PurchaseType == 2 || SalesType == 2) {
//                    TaxP = 0;
//                }
//                $("#tax_percentage_" + dataid).val(TaxP);
//                $("#base_rate_" + dataid).val(result.BasePrice);
//                $("#cfactor_" + dataid).val(result.ConFactor);

//                rowSubTotal(dataid);
//                CalculatetblItemListSum();
//                grandtotalcalculation();
//                paidamountcalculation();

//                if (action != "sales" || (action == "sales" && result.KeepStock != true) || (action == "sales" && result.KeepStock == true && result.total > 0)) {
//                    // append item unit list 

//                    $(selectObject).closest('tr').attr('class', "item_" + result.ItemID);
//                    if (action == "sales") {
//                        minstockupdate(result, dataid);
//                    }
//                    if ($(".item_").length == 0) {
//                        addrow('addinvoiceItem', '', '', '0.00', '0.00', '0');
//                    }
//                    $('.unit_name_' + dataid).focus();
//                }
//                else if ((result.KeepStock == true && result.CheckStock == 0 && result.total <= 0)) {

//                    var res = confirm("Are you Sure Want To Add Items In Less Stock ?");
//                    if (res == true) {
//                        $(selectObject).closest('tr').attr('class', "item_" + result.ItemID);
//                        if (action == "sales") {
//                            minstockupdate(result, dataid);
//                        }
//                        if ($(".item_").length == 0) {
//                            addrow('addinvoiceItem', '', '', '0.00', '0.00', '0');
//                        }
//                        $('.unit_name_' + dataid).focus();
//                    }
//                    else {
//                        $("#item_name_" + dataid).val(null).trigger("change");
//                        $("#unit_name_" + dataid).val(null).trigger("change");
//                        $("#total_qntt_" + dataid).val(null).trigger("change");
//                        $("#price_item_" + dataid).val(null).trigger("change");
//                    }
//                }
//                else {
//                    alert("This Item is Out of Stock!!!");
//                    var classname = $($("#total_qntt_" + dataid)).closest('tr').attr('class');
//                    if (classname != 'item_') {
//                        $("." + classname + " .btn-danger").click();
//                    }
//                    else {
//                        $("#item_name_" + dataid).val(null).trigger("change");
//                        $("#unit_name_" + dataid).val(null).trigger("change");
//                        $("#total_qntt_" + dataid).val(null).trigger("change");
//                        $("#price_item_" + dataid).val(null).trigger("change");
//                    }
//                }
//            }
//        });
//    }
//}
//check minimum stock
function minstockupdate(result, dataid) {
    var htdata = "<div class='minstock_" + dataid + "'";
    if (result.KeepStock == true) {
        totalstock = result.total;
        minstock = result.MinStock * result.ConFactor;
        htdata += " data-keeps='yes' data-min='" + minstock + "' data-confactor='" + result.ConFactor + "' data-stock='" + totalstock + "'>";
    }
    else {
        htdata += " data-keeps='no' >";
    }
    if ($(".minstock_" + dataid).length) {
        $(".minstock_" + dataid).remove();
    }
    $('#item_' + dataid).append(htdata);
}
function minstockcheck(arg) {
    var keepstock = $(".minstock_" + arg).attr('data-keeps');
    if (keepstock == "yes") {
        var index = $('#unit_name_' + arg).prop('selectedIndex');
        var unitname = $('#unit_name_' + arg).find('option:selected').text();
        var minstock = parseFloat($(".minstock_" + arg).attr('data-min'));
        var confactor = parseFloat($(".minstock_" + arg).attr('data-confactor'));
        var stock = parseFloat($(".minstock_" + arg).attr('data-stock'));
        var quantity = parseFloat($(".total_qntt_" + arg).val());

        var qty = 0;
        var classn = $("#item_" + arg).attr('class');

        $("." + classn).each(function () {

            var rowid = $(this).attr('id');
            var arr = rowid.split('_');
            var arg1 = arr[1];
            var index1 = $("#" + rowid + " .units").prop('selectedIndex');
            var curent = $("#" + rowid + " .quty").val();
            var confactor1 = parseFloat($("#" + rowid + "  .minstock_" + arg1).attr('data-confactor'));
            if (index == 0) {
                qty += (curent * confactor1);
            }
            else {
                qty += curent;
            }
        });
        if (index == 0) {
            //alert(stock);
            stock = stock - (qty - (quantity * confactor));
            //alert("stock = " + stock + " qty = " + qty + " quantity = " + quantity);
            minstock = minstock / confactor;
            stock = stock / confactor;
            var tostock = stock - quantity;
            var totstock = tostock / confactor;

            //var totstock = stock - qty;
            if (totstock <= minstock && totstock >= 0) {
                alert("Stock Exceeds Minimum Stock");
            }
            else if (quantity >= stock && stock <= 0) {
                $(".total_qntt_" + arg).val(quantity);
                stock = stock - (qty - quantity);
            }
            else if (totstock < 0) {
                stock = stock.toFixed(2);
                alert("This Item Is Going To Out of Stock!!! Only " + stock + " " + unitname + "Items Are Available In Stock.." + "");
                $(".total_qntt_" + arg).val(parseInt(stock));
            }

        } else {
            stock = stock - (qty - quantity);
            var totstock = stock - quantity;
            if (totstock <= minstock && totstock >= 0) {
                alert("Stock Exceeds Minimum Stock");
            }
            if (totstock < 0) {
                alert("This Item Is Going To Out of Stock!!! Only " + stock + " " + unitname + " Items Are Available In Stock..");
                $(".total_qntt_" + arg).val(stock);
            }

        }
        //if (quantity >= stock && stock <= 0) {
        //    alert("You are Adding The Item In Less Stock..");
        //    $(".total_qntt_" + arg).val(quantity);
        //    stock = stock - (qty - quantity);
        //}
    }
}
// create units based on primary and secondary
function createUnitList(result, dataid, action) {
    // clear previous content
    if (action == "sales" || action == "quot" || action == "foredit") {
        var ROnlyRate = $("#ROnlyRate").val();
        if (ROnlyRate == "active") {
            $("#price_item_" + dataid).attr('readonly', true);
        }
    }
    $('#unit_name_' + dataid).empty();
    if (result.ItemUnitID != null) {
        var newOption = $('<option></option>');
        if ((result.PriUnit != result.SubUnit) && result.SubUnitId != null) {
            newOption.val(result.ItemUnitID).html(result.PriUnit);

            var newOption1 = $('<option></option>');
            newOption1.val(result.SubUnitId).html(result.SubUnit);
            if (result.ItemUnit) {
                if (result.ItemUnit == result.ItemUnitID)
                    newOption.attr("selected", "selected");
                if (result.ItemUnit == result.SubUnitId)
                    newOption1.attr("selected", "selected");
            }

            $('#unit_name_' + dataid).append(newOption);
            $('#unit_name_' + dataid).append(newOption1);
        }
        else {
            newOption.val(result.ItemUnitID).html(result.PriUnit);
            $('#unit_name_' + dataid).append(newOption);
        }
    }
    else {

    }
}
// search item
function searchItem() {
    var selecteditem = new Array();

    $(".item_name").each(function () {
        selecteditem.push($(this).val());
    });
    var mc = $("#ddlMC").val();
    var Inv = $('#ddlInvoice').val();

    var newurl;
    if (Inv != null) {
        newurl = "/HireReturn/SearchInvoiceItem"
    }
    else {
        newurl = "/Item/SearchdetailsMC"
    }

    if (mc != null && mc > 0) {
        $(".item_name").select2({
            placeholder: 'Search Item by Code',
            minimumInputLength: 0,
            ajax: {
                url: newurl,
                dataType: 'json',
                type: "POST",
                delay: 50,
                data: function (params) {
                    return {
                        q: params.term || "",
                        cust: $("#ddlCustomer").val(),
                        ItemID: selecteditem,
                        page: params.page || 1,
                        mc: mc,
                        constat: $("#ContType").val()
                        , Invoice: $('#ddlInvoice').val()
                    };
                },
                processResults: function (data, params) {
                    // parse the results into the format expected by Select2
                    // since we are using custom formatting functions we do not need to
                    // alter the remote JSON data, except to indicate that infinite
                    // scrolling can be used
                    params.page = params.page || 1;

                    return {
                        results: data,
                        pagination: {
                            //more: (params.page * 10) < 50
                            more: true
                        }
                    };
                },
                cache: true
            },
            templateResult: repoFormatResult,
            templateSelection: repoFormatSelection,
        });
    } else {
        $(".item_name").select2({
            placeholder: 'Search Item by Code',
            minimumInputLength: 0,
            ajax: {
                url: "/Item/Search",
                dataType: 'json',
                type: "POST",
                delay: 50,
                data: function (params) {
                    return {
                        q: params.term || ""

                    };
                },
                processResults: function (data, params) {
                    // parse the results into the format expected by Select2
                    // since we are using custom formatting functions we do not need to
                    // alter the remote JSON data, except to indicate that infinite
                    // scrolling can be used
                    params.page = params.page || 1;

                    return {
                        results: data,
                        pagination: {
                            //more: (params.page * 10) < 50
                            more: true
                        }
                    };
                },
                cache: true
            },
            templateResult: repoFormatResult,
            templateSelection: repoFormatSelection,
        });
    }
    rate_change(count);
}
function searchproject() {
    $(".project_name").select2({
        placeholder: 'Search Project Code Or Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/Project/SearchProject",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "empty"
                };
            },
            processResults: function (data, params) {
                params.page = params.page || 1;
                return {
                    results: data
                };
            },
            cache: true
        },
    });
}
function searchtask(dataid) {
    var project = $("#project_name_" + dataid).val();
    $("#task_name_" + dataid).val(null).trigger('change');
    $(".task_name").select2({
        placeholder: 'Search Task Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/ProTask/SearchTaskByProject",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    project: project,
                    page: params.page
                };
            },
            processResults: function (data, params) {
                params.page = params.page || 1;
                return {
                    results: data
                };
            },
            cache: true
        },
        // templateResult: projectFormatResult,
        // templateSelection: projectFormatSelection,
    });
}

function searchbrand() {
    $(".brand_name").select2({
        placeholder: 'Search Item Brand',
        minimumInputLength: 0,
        ajax: {
            url: "/ItemBrand/SearchBrand",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "empty"
                };
            },
            processResults: function (data, params) {
                params.page = params.page || 1;
                return {
                    results: data
                };
            },
            cache: true
        },
    });
}

function GetProjectChange(selectObject, dataid, action) {
    searchtask(dataid);
}

function repoFormatResult(repo) {
    var bg = "";
    if (repo.KeepStock) {
        bg = (parseFloat(repo.total) > 0) ? "" : " text-red";
    }
    var markup = '<div class="se-row' + bg + '">' +
             '<h4>' + repo.text + '</h4>';
    if (repo.PartNumber != "" && repo.PartNumber != null) {
        markup += '<div class="se-sec">Part No : ' + repo.PartNumber + '</div>,';
    }
    if (repo.price)
        markup += '<div class="se-sec">Price:' + parseFloat(repo.price).toFixed(2) + '</div>';
    if (repo.cost)
        markup += '<div class="se-sec">Cost:' + parseFloat(repo.cost).toFixed(2) + '</div>';

    if (repo.KeepStock) {
        var total;
        var primary = (repo.total / repo.ConFactor);
        if (repo.total % repo.ConFactor == 0) {
            total = (repo.total / repo.ConFactor) + " " + repo.PriUnit;
        }
        else {
            var p = parseInt(((repo.total / repo.ConFactor) * 100) / 100);
            var sub = (repo.total % repo.ConFactor).toFixed(0);
            total = p + " " + repo.PriUnit + ", " + sub + " " + repo.SubUnit
        }
        if (primary < repo.MinStock) {
            markup += '<div class="se-sec text-yellow">Availability :' + total + '</div>';
        }
        else {
            markup += '<div class="se-sec">Availability :' + total + '</div>';
        }
    }
    if (repo.lastSale && repo.lastSale != null) {
        markup += '<div class="se-sec">Last Sale Price:' + parseFloat(repo.lastSale).toFixed(2) + '</div>';
    }
    if (repo.lastPur && repo.lastPur != null) {
        markup += '<div class="se-sec">Last Purchase Price:' + parseFloat(repo.lastPur).toFixed(2) + '</div>';
    }

    markup += '</div>';
    var retn = $(markup);
    return retn;
}

function repoFormatSelection(repo) {
    return repo.text;
}

function quantity_change(arg) {

    //if ($('total_qntt_' + arg).val() > 0) {
    //minstockcheck(arg);
    rowSubTotal(arg);
    CalculatetblItemListSum();
    grandtotalcalculation();
    paidamountcalculation();
    Discounts();
}
function rate_change(arg, type, foredit) {
    //minstockcheck(arg);
    var baserate = $("#base_rate_" + arg).val();
    var rate = $(".price_item_" + arg).val();

    if (parseFloat(baserate) > parseFloat(rate) && type == 'sales' && parseFloat(rate) > 0 && foredit != 'foredit') {
        alert("Selling price is less than Base Price ");
    }

    rowSubTotal(arg);
    CalculatetblItemListSum();
    grandtotalcalculation();
    paidamountcalculation();
}
function itemdiscount_change(arg) {


    rowSubTotal(arg);
    CalculatetblItemListSum();
    grandtotalcalculation();
    paidamountcalculation();
    DiscountChng(arg);
}

//function discount_change(arg) {
//    grandtotalcalculation();
//}
function paidamount_change() {
    //CalculatetblItemListSum();
    paidamountcalculation();
}

function rowSubTotal(arg) {
    var tax = $("#tax_percentage_" + arg).val();
    var quantity = $(".total_qntt_" + arg).val();
    var rate = $(".price_item_" + arg).val();
    //alert("price : " + rate + " arg =" + arg);
    var subtotal = quantity * rate;
    $(".sub_total_" + arg).val(subtotal.toFixed(2));
    var itemdiscount = $(".item_discount" + arg).val();
    subtotal = subtotal - itemdiscount;

    //var taxAmount = subtotal * (tax / 100);
    var taxAmount = (Math.round(subtotal * tax) / 100);
    var Total = subtotal + taxAmount;

    $("#tot_tax_" + arg).val(taxAmount.toFixed(2));
    $(".tax_" + arg).val(taxAmount.toFixed(2) + " (" + tax + "%)");
    $(".total_price_" + arg).val(Total.toFixed(2));
}

function paidamountcalculation() {
    var paidAmt = $("#PaidAmount").val();
    var gdTotal = $("#GrandTotal").val();
    if (gdTotal != "" && $("#PaidAmount").is(":visible")) {
        if (parseFloat(gdTotal) < parseFloat(paidAmt)) {
            alert("paid should less than or equals total amount");
            $("#PaidAmount").val("0.00");
            $("#DueAmount").val("0.00");
        }
        else {
            var dues = gdTotal - paidAmt;
            $("#DueAmount").val(parseFloat(dues).toFixed(2));
        }
    }
}
function CalculatetblItemListSum() {
    var tax = $(".tot_tax").val();
    var qty = $(".ItemQty").val();
    if (tax > 0 || qty != 0) {
        var tbody = $("#normalinvoice tbody");
        if (tbody.children().length > 0) {
            var gtTax = 0;
            var gtTotal = 0;
            var gtQty = 0;
            var gtSubTotal = 0;
            var gtDiscount = 0;
            var gtRate = 0;
            $(".tot_tax").each(function () {
                var indTax = $(this).val();

                gtTax = parseFloat(gtTax) + parseFloat(indTax);
            });

            $("[id$=total_tax_amount]").text(parseFloat(gtTax).toFixed(2));
            $(".total_price").each(function () {
                var indtot = $(this).val();
                gtTotal = parseFloat(gtTotal) + parseFloat(indtot);
            });
            $(".quty").each(function () {
                var subQty = this.value;
                gtQty = parseFloat(gtQty) + parseFloat(subQty);
            });

            $(".subtotal").each(function () {
                var subTot = this.value;
                gtSubTotal = parseFloat(gtSubTotal) + parseFloat(subTot);
            });
            $(".item_discount").each(function () {
                var subDisc = this.value;
                gtDiscount = parseFloat(gtDiscount) + parseFloat(subDisc);
            });
            gtDiscount = gtDiscount || 0.00;

            //$(".totrate").each(function () {
            //    var subRate = parseFloat(this.value);
            //    gtRate += parseFloat(subRate);
            //});

            // $("#GrandTotal").val(parseFloat(gtTotal).toFixed(2));
            $("[id$=TotRate]").text((gtRate).toFixed(2));
            $("[id$=total]").text((gtTotal).toFixed(2));
            $("[id$=ItemCount]").val(tbody.children().length);
            $("[id$=ItemQty]").text((gtQty).toFixed(2));
            $("[id$=SubTotal]").text((gtSubTotal).toFixed(2));
            $("[id$=ItemDisc]").text(parseFloat(gtDiscount).toFixed(2));
        }
    }
}
//item unit change
function unitchange(selectObject, arg, action) {
    //minstockcheck(arg);
    var index = $('#unit_name_' + arg).prop('selectedIndex');

    if (index == 1) {
        var unitId = parseFloat($('#unit_name_' + arg).val());
        var cfactor = parseFloat($('#cfactor_' + arg).val());
        var price = parseFloat($("#base_rate_" + arg).attr("data-value"));
        var newprice = parseFloat(price / cfactor);
        $(".price_item_" + arg).val(newprice.toFixed(2));
    } else {
        var unitId = parseFloat($('#unit_name_' + arg).val());
        //var cfactor = parseFloat($('#cfactor_' + arg).val());
        var price = parseFloat($("#base_rate_" + arg).attr("data-value"));
        //var newprice = parseFloat(price * cfactor);
        $(".price_item_" + arg).val(price.toFixed(2));
    }

    rowSubTotal(arg);
    CalculatetblItemListSum();
    grandtotalcalculation();
    paidamountcalculation();
}

//Delete a row of table
function deleteRow(t) {
    var classname = $(t).closest('tr').attr('class');
    if (classname == 'item_') alert("Sorry you can't delete this row.");
    else {
        var r = confirm("Are you sure you want to delete this..?");
        if (r == true) {
            var e = t.parentNode.parentNode;
            e.parentNode.removeChild(e);
        }
    }
    CalculatetblItemListSum();
    grandtotalcalculation();
    paidamountcalculation();
    var i = 1;
    $('#addinvoiceItem tr').each(function () {
        $(this).find('td:first').text(i);
        i++;
    });
}

var bcount = 1, btype = '';
blimits = 50;
function addbillsundry(t, action, BsValue, AmountType, BsAmount, BsType, BSName, billsundry) {
    if (bcount == blimits) alert("You have reached the limit of adding " + bcount + " inputs");
    else {
        var data = "";
        var Type = "";
        var Option = "";
        var readonly = "";
        var row = "<tr class='bs_'>";
        var slno = $('#addbillsundry tr').length + 1;
        tabindex = bcount * 5;
        tab1 = tabindex + 1;
        tab2 = tabindex + 2;
        tab3 = tabindex + 3;
        tab4 = tabindex + 4;
        tab5 = tabindex + 5;
        if (billsundry != null) {
            row = "<tr class='bs_" + billsundry + "'>";
            Option = "<option value='" + billsundry + "'>" + BSName + "</option>";
        }

        if (AmountType == 1) {
            Type = "%";
        } else {
            Type = "";
        }
        if (BsValue == null) {
            BsValue = "";
            readonly = "readonly";
        }

        data = "<td class='text-center'>" + slno + "</td>" +
           "<td class='input-group input-group-sm'><select data-name='BillSundry' class='form-control bsname' data-id='" + bcount + "' placeholder='Bill Sundry Name' id='bsname'  data-val-required='The bill sundry name field is required' onchange='GetBillSundrydetails(this," + bcount + ")'>" + Option + "</select></td>" +
           "<td><input type='number' data-name='BsValue' " + readonly + " value='" + BsValue + "'  class='form-control bsvalue_" + bcount + "' onchange='bsvaluechange(" + bcount + ");' id='bsvalue_" + bcount + "' data-id='" + bcount + "' id='bsvalue' /></td>" +
           "<td><input type='text' data-name='' value='" + Type + "' class='form-control bsamttype_" + bcount + "' id='bsamttype_" + bcount + "' data-id='" + bcount + "' id='bsamttype' readonly='readonly'/></td>" +
           "<td><input type='number' data-name='BsAmount' value='" + BsAmount + "' class='form-control bsamt bsamt_" + bcount + "' onchange='bsamtchange(" + bcount + ");' id='bsamt_" + bcount + "' data-id='" + bcount + "' id='bsamt' value='0.00' placeholder='0.00'/><input type='hidden' data-name='AmountType'  value='" + AmountType + "' class='amttypevalue' name='amttypevalue' id='amttypevalue_" + bcount + "'/><input type='hidden' value='" + BsType + "' data-name='BsType'  class='bstype' name='bstype' id='bstype_" + bcount + "'/></td>" +
           "<td class='text-center'><button style='text-align: right;' class='btn btn-danger' type='button' value='Delete' onclick='deletebsRow(this)'><i class='fa fa-trash fa-1x'></i></button></td>",
        row += data + "</tr>";
        $('#' + t).append(row);
        searchbs();
        bcount++;
        setTabIndex();
    }
}
function searchbs() {

    var selecteditem = new Array();
    $(".bsname").each(function () {
        selecteditem.push($(this).val());
    });

    $(".bsname").select2({
        placeholder: 'Search Bill Sundry',
        minimumInputLength: 0,
        ajax: {
            url: "/BillSundry/Search",
            dataType: 'json',
            type: "POST",
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    ItemID: selecteditem,
                    page: params.page
                };
            },
            processResults: function (data, params) {
                params.page = params.page || 1;

                return {
                    results: data
                };
            },
            cache: true
        },
    });

}
function GetBillSundrydetails(selectObject, dataid) {
    var SbId = selectObject.value;
    if (SbId != null) {
        if ($(".bs_" + SbId).length > 0) {
            if (confirm('Are you sure want to Add this Bill Sundry Again?')) {
                bsUpdate(selectObject, dataid);
            }
        }
        else {
            bsUpdate(selectObject, dataid);
        }
    }
}
function bsUpdate(selectObject, dataid) {
    $.ajax({
        url: '/BillSundry/GetBillSundryById',
        type: "GET",
        dataType: "JSON",
        data: { bsID: selectObject.value },
        success: function (result) {
            //additive/subtrative
            $("#bstype_" + dataid).val(result.BSType);

            //percentage/amt
            $("#amttypevalue_" + dataid).val(result.AmountType);
            $("#bsvalue_" + dataid).val(result.DefaultValue);
            var defvalue = $("#bsvalue_" + dataid).val();

            BindBsAmount(dataid, defvalue);
            grandtotalcalculation();


            $(selectObject).closest('tr').attr('class', "bs_" + result.BillSundryId);
            if ($(".bs_").length == 0) {
                addbillsundry('addbillsundry', '', '0.00', '', '0.00', '');
            }

        }
    });
}
//percentage cal on billsundry
function calculatePercentage(dataid) {

    var gtTotal = parseFloat($("#total").text());
    var taxAmt = parseFloat($("#total_tax_amount").text());

    gtTotal = (gtTotal > 0) ? gtTotal : 0;
    $("#addbillsundry tr").each(function () {
        var type = parseFloat($(this).find('.bstype').val());
        var amt = $(this).find('.bsamt').val();
        amt = (amt > 0) ? amt : 0;
        if (type == 0) {
            gtTotal = parseFloat(gtTotal) + parseFloat(amt);
        } else if (type == 1) {
            gtTotal = parseFloat(gtTotal) - parseFloat(amt);
        }
    });

    gtTotal = parseFloat(gtTotal) - parseFloat(taxAmt);
    gtTotal = (gtTotal > 0) ? gtTotal : 0;
    var value = parseFloat($("#bsvalue_" + dataid).val());
    var amt = (gtTotal * (value / 100));
    $("#bsamt_" + dataid).val(amt.toFixed(2));
}

function BindBsAmount(dataid, defvalue) {
    var value = parseFloat($("#bsvalue_" + dataid).val());
    var bstype = parseFloat($("#bstype_" + dataid).val());
    var amtype = $("#amttypevalue_" + dataid).val();
    var total = parseFloat($("#total").text());


    total = (total > 0) ? total : 0;
    if (amtype == 0) {
        $("#bsvalue_" + dataid).val("").attr('readonly', true);
        $("#bsamttype_" + dataid).val("");
        $("#bsamt_" + dataid).val(parseFloat(defvalue).toFixed(2));
        $("#bsamt_" + dataid).focus();
    } else {
        $("#bsvalue_" + dataid).focus();
        $("#bsvalue_" + dataid).val(defvalue);
        $("#bsamttype_" + dataid).val("%");
        $("#bsvalue_" + dataid).attr('readonly', false);
        calculatePercentage(dataid);
    }
    grandtotalcalculation();
}
function grandtotalcalculation() {
    var gtTotal = parseFloat($("#total").text());
    gtTotal = (gtTotal > 0) ? gtTotal : 0;
    $("#addbillsundry tr").each(function () {
        var type = parseFloat($(this).find('.bstype').val());
        var amt = $(this).find('.bsamt').val();

        amt = (amt > 0) ? amt : 0;
        if (type == 0) {
            gtTotal = parseFloat(gtTotal) + parseFloat(amt);
        } else if (type == 1) {

            gtTotal = parseFloat(gtTotal) - parseFloat(amt);
        }
    });
    var stype = $("#PurchaseType").val();
    if (stype == 3) {
        var gtTaxAmt = parseFloat($("#total_tax_amount").text());
        $("#GrandTotal").val((parseFloat(gtTotal) - parseFloat(gtTaxAmt)).toFixed(2));
    } else {
        $("#GrandTotal").val(parseFloat(gtTotal).toFixed(2));
    }
    FCCalculation();
    paidamountcalculation();
}

//onchange of billsundry value
function bsvaluechange(arg) {
    var defvalue = $("#bsvalue_" + arg).val();
    BindBsAmount(arg, defvalue);
}
//amt chnage
function bsamtchange(arg) {
    var defvalue = parseFloat($("#bsamt_" + arg).val());
    BindBsAmount(arg, defvalue);
}

//Delete a row of table
function deletebsRow(t) {
    var classname = $(t).closest('tr').attr('class');
    if (classname == 'bs_') alert("Sorry You Can't Delete This Row.");
    else {
        var r = confirm("Are you sure you want to delete this..?");
        if (r == true) {
            var e = t.parentNode.parentNode;
            e.parentNode.removeChild(e);
        }
    }
    grandtotalcalculation();
    var i = 1;
    $('#addbillsundry tr').each(function () {
        $(this).find('td:first').text(i);
        i++;
    });
}

//print item bill sundry
//itembind
function bindItem(e, dvitem) {
    var total = parseFloat(0);
    var str = "";
    var itemcode = "";
    var count = 1;
    var qty = 0;
    var wgt = 0;
    var cbm = 0;
    var Layout = (typeof e.layout == 'undefined') ? "Default" : e.layout.Name;
    var UnitName = "";
    var TotTaxableAmount = 0;
    var TotTaxAmount = 0;
    var GrandTot = 0;
    var QtyTot = 0;
    var itSubtotal = 0;
    $("#PoNo").hide();
    function ItemsBind(ritem, rtype, bcount) {
        var itSubtotal = parseFloat(ritem.ItemSubTotal);
        var itDiscount = parseFloat(ritem.ItemDiscount);
        var itTaxable = itSubtotal - itDiscount;
        var TaxableAmount = (Layout != "Scaffold") ? parseFloat(ritem.ItemSubTotal).toFixed(2) : itTaxable.toFixed(2);
        TotTaxAmount += rtype != "bundle" ? ritem.ItemTaxAmount : 0;
        TotTaxableAmount += rtype != "bundle" ? parseFloat(TaxableAmount) : 0;
        GrandTot += rtype != "bundle" ? ritem.ItemTotalAmount : 0;
        QtyTot += (rtype != "bundle" && ritem.KeepStock) ? ritem.ItemQuantity : 0;
        var Row = "";
        var unit = (ritem.ItemUnit != null) ? ritem.ItemUnit : "";
        var PartNo = (ritem.PartNumber != null && ritem.PartNumber != "") ? ritem.PartNumber : "";
        var itemnote = "";
        if (ritem.ItemNote != "" && ritem.ItemNote != "-:{Bundle_Item}") {
            itemnote = "<br /><small>" + ritem.ItemNote + "</small>";
        }
        itemnote = itemnote.replaceAll('<p></p>', '');
           
            itemnote = itemnote.replaceAll('<p><br /></p>', '');
            itemnote = itemnote.replaceAll('<p><br></p>', '');
            itemnote = itemnote.replaceAll('<br />', '');
            itemnote = itemnote.replaceAll('<br></p>', '</p>');
            itemnote = itemnote.replaceAll('</p><br>', '</p>');
            itemnote = itemnote.replaceAll('<b><br></b>', '');
            itemnote = itemnote.replaceAll('</ul><br>', '</ul>');
        var dvField1 = "";
        var dvField2 = "";
        var trcount = rtype == "bundle" ? bcount : count;
        var bold = (Layout != "Scaffold") ? "" : "";
        var boldend = (Layout != "Scaffold") ? "</b>" : "";
        if (dvitem != "active" && rtype != "bundle") {
            dvField1 += '<td class="text-right">' + bold + parseFloat(ritem.ItemUnitPrice).toFixed(2) + boldend + '</td>';
            dvField1 += '<td class="text-right">' + bold + TaxableAmount + boldend + '</td>';
            dvField2 += '<td class="text-right">' + bold + parseFloat(ritem.ItemTaxAmount).toFixed(2) + boldend + '</td>';
            dvField2 += '<td class="text-right">' + bold + parseFloat(ritem.ItemTotalAmount).toFixed(2) + boldend + '</td>';
        } else if (dvitem != "active" && rtype == "bundle") {
            dvField1 += '<td></td><td></td>';
            dvField2 += '<td></td><td></td>';
        }
        Row += (Layout != "Scaffold") ? '<tr class="noborder">' : '<tr class="border-top">';
        Row += '<td>' + trcount + '</td>';
        

        if (Layout == "Jewellery") {
            Row += '<td>' + ritem.ItemCode + '</td>';
            Row += '<td>' + ritem.ItemName + itemnote + '</td>';
            Row += '<td>' + ritem.ItemQuantity + '</td>';
            Row += '<td>' + ritem.ItemQuantity + '</td>';
            Row += dvField1;
        }
        else if (Layout == "Scaffold") {
            var CBM = (ritem.CBM != null && ritem.CBM != "") ? (parseFloat(ritem.CBM) * parseFloat(ritem.ItemQuantity)).toFixed(2) : "";
            var Weight = (ritem.Weight != null && ritem.Weight != "") ? (parseFloat(ritem.Weight) * parseFloat(ritem.ItemQuantity)).toFixed(2) : "";
            var img = "";
            wgt = parseFloat(wgt) + parseFloat(Weight || 0);
            cbm = parseFloat(cbm) + parseFloat(CBM || 0);
            if (ritem.img != null && ritem.img.length > 0) {
                $.each(ritem.img, function (j, imgs) {
                    var im = "/uploads/itemimages/" + ritem.Id + "/thumb_" + imgs.FileName;
                    img = "<img width='68' height='46' src='/uploads/itemimages/" + ritem.Id + "/thumb_" + imgs.FileName + "'/>";
                    // img = "<div style='width:50px;height:50px;background:url(" + im + ");background-size: cover;'></div>";
                });
            }
            var itnamecols = (Weight == "") ? ((CBM == "") ? 3 : 2) : 1;
            //  console.log("weight :"+Weight+" CBM:"+CBM+" cols:"+itnamecols);
            if (img == "") {
                itnamecols++;
            }
            //  console.log(" IMG:" + img + " cols:" + itnamecols);
            if (rtype != "bundle") {
                if (ritem.ItemDescription != "" && ritem.ItemDescription != null) {
                    itemnote += "<br /><small>" + ritem.ItemDescription + "</small>";
                }
                Row += '<td colspan="' + itnamecols + '"><b>' + ritem.ItemName + "</b>" + itemnote + '</td>';
            } else {
                Row += '<td colspan="' + itnamecols + '"><i style="color: #747474 !important;">' + ritem.ItemName + itemnote + '</i></td>';
            }
            if (img != "") {
                Row += '<td style="width:70px; padding:1px;">' + img + '</td>';
            }

            if (rtype != "bundle") {
                if (Weight != "") {
                    Row += '<td><b>' + Weight + '</b></td>';
                    Row += '<td><b>' + CBM + '</b></td>';
                }
                if (Weight == "" && CBM != "") {
                    Row += '<td><b>' + CBM + '</b></td>';
                }
                Row += '<td><b>' + ritem.ItemQuantity + ' ' + unit + '</b></td>';
            } else {
                if (Weight != "") {
                    Row += '<td>' + Weight + '</td>';
                    Row += '<td>' + CBM + '</td>';
                }
                if (Weight == "" && CBM != "") {
                    Row += '<td>' + CBM + '</td>';
                }
                Row += '<td>' + ritem.ItemQuantity + ' ' + unit + '</td>';
            }
            Row += dvField1;
            if (dvitem != "active" && rtype == "bundle") {
                Row += '<td></td>';
            }
            else {
                Row += '<td class="text-right">' + parseFloat(ritem.ItemTax).toFixed(0) + '</td>';
            }
            Row += dvField2;

        }

        else if (Layout == "DotMatrix") {
            Row = '<tr class="noborder">';
            Row += '<td class="text-center">' + trcount + '</td>';
            var itqty = "";
            var itkg = "";
            var itkgrtr = "";

                itkgrtr = ritem.ItemUnitPrice;
            if (unit == "Kgs.") {
                itkg = ritem.ItemQuantity;
            }
            else {
                itqty = ritem.ItemQuantity;
            }


            var index = TaxableAmount.indexOf(".");
            var firstValue = TaxableAmount.substr(0, index);
            var secValue = TaxableAmount.substr(index + 1);

            var itname = ritem.ItemName;
            if (ritem.ItemName == "ITEM") {
                itname = ritem.ItemNote;
            }
            Row += '<td class="text-center">' + itqty + '</td>';
            Row += '<td>' + itname + '</td>';
            Row += '<td>' + itkg + '</td>';
            Row += '<td>' + itkgrtr + '</td>';
            Row += '<td class="text-right">' + firstValue + '</td>';
            Row += '<td>' + secValue + '</td>';

        }
        else {
            // Default Invoice Structure
            //if (Layout == "Default" || Layout == "General")
            if (e.summary.chkCode == 0) {
                itemcode = ritem.ItemCode + " - ";
            }
            Row += '<td>' + ritem.ItemCode  + '</td>';
            Row += '<td>' + ritem.ItemName + itemnote + '</td>';
            Row += '<td>' + unit + '</td>';
            Row += '<td>' + ritem.ItemQuantity + '</td>';
            Row += dvField1 + dvField2;
        }

        Row += '</tr>';
        return Row;
    }
    $.each(e.item, function (i, item) {
        qty += item.ItemQuantity;

        var subtot = parseFloat(item.ItemTotalAmount.toFixed(2));
        total += subtot;

        str += ItemsBind(item);
        count++;
        // bundle items
        if (item.bundle != null && item.bundle.length > 0) {
            $.each(item.bundle, function (j, itemss) {
                var bcount = j + 1
                str += ItemsBind(itemss, "bundle", bcount);
            });
        }
    });
    if (Layout == "Jewellery") {
        str += '<tr id="jwltotal" class="border-top"><td colspan="2"><b>(' + (count - 1) + ' items)</b></td><td class="text-center"><b> Total الجمالى</b></td>';
        str += '<td><b>' + parseFloat(qty).toFixed(2) + '</b></td><td><b>' + parseFloat(qty).toFixed(2) + '</b></td><td></td><td class="text-right"><b>' + parseFloat(total).toFixed(2) + '</b></td></tr>';
    }
    if (Layout == "Scaffold") {
        var weihtv = (parseFloat(wgt) != 0) ? parseFloat(wgt).toFixed(2) : "";
        var cbmv = (parseFloat(cbm) != 0) ? parseFloat(cbm).toFixed(2) : "";
        str += "<tr class='border-top'><td colspan='3' class='text-right'><b>TOTAL</b></td><td class='text-center'><b>" + weihtv + "</b></td><td class='text-center'><b>" + cbmv + "</b></td><td></td><td></td><td></td><td colspan='2'></td><td></td></tr>";
        str += "<tr class='border-top'><td colspan='5' class='text-right'><b>TOTAL</b></td><td class='text-right'>" + parseFloat(QtyTot).toFixed(2) + "</td><td></td><td class='text-right'>" + parseFloat(TotTaxableAmount).toFixed(2) + "</td><td colspan='2' class='text-right'>" + parseFloat(TotTaxAmount).toFixed(2) + "</td><td class='text-right'><b>" + parseFloat(GrandTot).toFixed(2) + "</b></td></tr>";
    }
    if (Layout == "General") {
        str += '<tr id="gentotal" class="border-top"><td colspan="2"><b>(' + (count - 1) + ' items)</b></td><td class="text-center"><b> Total</b></td>';
        str += '<td><b>' + parseFloat(qty).toFixed(2) + '</b></td><td></td><td class="text-right"><b>' + parseFloat(TotTaxableAmount).toFixed(2) + '</b></td><td class="text-right"><b>' + parseFloat(TotTaxAmount).toFixed(2) + '</b></td><td class="text-right"><b>' + parseFloat(total).toFixed(2) + '</b></td></tr>';
    }
    if (Layout == "DotMatrix") {
        var totamt = parseFloat(TotTaxableAmount).toFixed(2);
        var index1 = totamt.indexOf(".");
        var firstValue1 = totamt.substr(0, index1);
        var secValue1 = totamt.substr(index1 + 1);
        str += '<tr id="gentotal"><td></td><td></td><td></td><td></td><td></td><td class="text-right"><bb>' + firstValue1 + '</b></td><td><bb>' + secValue1 + '</bb></td></tr>';
        var totamttax = parseFloat(TotTaxAmount).toFixed(2);
        var index2 = totamttax.indexOf(".");
        var firstValue2 = totamttax.substr(0, index2);
        var secValue2 = totamttax.substr(index2 + 1);
        str += '<tr><td></td><td></td><td></td><td></td><td></td><td class="text-right"><bb>' + firstValue2 + '</bb></td><td><bb>' + secValue2 + '</bb></td></tr>';
        var totamts = parseFloat(total).toFixed(2);
        var index3 = totamts.indexOf(".");
        var firstValue3 = totamts.substr(0, index3);
        var secValue3 = totamts.substr(index3 + 1);
        var word1 = conNumber(parseFloat(total));
        str += '<tr><td colspan="4" style="padding-left:50px;font-size:12px;"><div style="position: absolute;width:500px;margin-top:-5px;">' + word1 + '</div></td><td></td><td class="text-right"><bb>' + firstValue3 + '</bb></td><td><bb>' + secValue3 + '</bb></td></tr>';

        // str += '<td><b>' + parseFloat(qty).toFixed(2) + '</b></td><td></td><td class="text-right"><bb>' + parseFloat(TotTaxableAmount).toFixed(2) + '</bb></td><td><bb>' + parseFloat(TotTaxAmount).toFixed(2) + '</bb></td><td class="text-right"><bb>' + parseFloat(total).toFixed(2) + '</bb></td></tr>';
    }
    return str;
}



// bind bill sundry
function bindSundry(e) {
    var str = "";
    var Layout = (typeof e.layout == 'undefined') ? "Default" : e.layout.Name;
    $.each(e.billsundry, function (i, billsundry) {
        var type = "";
        var type2 = "";
        var symbol = "";
        var value = "";
        if (billsundry.BsType == 0) {
            type = "Add";
        } else {
            type = "Less";
        }
        if (billsundry.AmountType == 1) {
            type2 = "&#64;";
            symbol = "%";
        } else {
            type2 = "";
            symbol = "";
        }
        if (billsundry.BsValue > 0) {
            value = parseFloat(billsundry.BsValue).toFixed(2);
        } else {
            value = "";
        }
        if (Layout == "Scaffold") {
            str += "<tr class='border-top'><td colspan='5'></td><td class='text-right'>" + billsundry.BillSundry + "</td><td class='text-center'>" + parseFloat(billsundry.BsAmount).toFixed(2) + "</td></tr>";
        }
        else {

            str += "<tr class='border-top'>";
            str += '<td>' + billsundry.BillSundry + '</td>';
            str += '<td class="text-right">' + parseFloat(billsundry.BsAmount).toFixed(2) + '</td>';
            str += '</tr>';
        }

    });
    return str;
}

function PrintInvoice(e, type, dvitem, conType) {
    var Layout = (typeof e.layout == 'undefined') ? "Default" : e.layout.Name;
    var Bill_Total = $("#Bill_Total").html();
    var Bill_Tax = $("#Bill_Tax").html();
    var Bill_Discount = $("#Bill_Discount").html();
    var Terms = $("#Terms").html();
    var Bill_Amount = $("#Bill_Amount").html();
    if (typeof Bill_Total === 'undefined') {
        Bill_Total = "Total <span style='direction:ltr'>(AED)</span>";
    }
    if (typeof Bill_Tax === 'undefined') {
        Bill_Tax = "VAT(5.00%)";
    }
    if (typeof Bill_Discount === 'undefined') {
        Bill_Discount = "Discount  ";
    }
    if (typeof Terms === 'undefined') {
        Terms = "Terms And Conditions";
    }
    if (typeof Bill_Amount === 'undefined') {
        Bill_Amount = "Amount  ";
    }
    if (e.summary.ProCheck == 0 && (e.summary.PrjNameCode != null && e.summary.PrjNameCode != "")) {
        $("#lblProject").text(e.summary.PrjNameCode);
        $("#divproject").show();
    }
    else {
        $("#divproject").hide();
    }
    $("#lblBillNo").text(e.summary.BillNo);
    $("#lblDate").text(convertToDate(e.summary.Date));
    if (e.summary.paytype != null) {
        $("#lblpaytype").text(e.summary.paytype);
    } else {
        $("#Mpaytype").hide();
    }
    if (e.summary.PONo != null) {
        $("#lblPONo").text(e.summary.PONo);
    }
    else {
        $("#lpoSet").addClass("hidden");
    }
    if (dvitem == "active") {
        $(".itemdata").hide();
    }

    if (e.summary.Location != null) {
        $("#lblLocation").text(e.summary.Location);
    } else {
        $("#LocLbl").hide();
    }

    if (e.summary.Cashier != "  ") {
        $("#lblEmployee").text(e.summary.Cashier);
    } else {
        $("#empSet").addClass("hidden");
    }
    if (type == "sales") {

        if (e.summary.PONo != "") {
            $("#lblPONo").text(e.summary.PONo);
        } else {
            $("#ponoSet").addClass("hidden");
        }
        var hidCount = $("#Cust_de .hidden").length;
        if (hidCount < 2) {
            $("#Cust_de").css("font-size", "0.9em");
        }
        //console.log(hidCount);
    }
    if (e.summary.paytype == "Credit" && e.summary.CreditPeriod != 0 && e.summary.CreditPeriod != null) {
        $("#lblcperiod").text(e.summary.CreditPeriod + " Days");
    }
    else {
        $("#crperiod").addClass("hidden");
    }
    if (e.summary.ConvertType == "DVNote") {
        $("#lbldvn").text(e.summary.ConvertNo);
    }
    else {
        $("#dvnSet").addClass("hidden");
    }

    if (typeof e.summary.AgainstInvoice != 'undefined' && e.summary.AgainstInvoice != null) {
        $("#agstInvoice").text(e.summary.AgainstInvoice);
    }
    else {
        $("#divAgstInvoice").addClass("hidden");
    }
    var tc = (e.summary.Note != "" && e.summary.Note != null && typeof e.summary.Note != 'undefined') ? e.summary.Note.replace(/\n/g, "<br/>") : "";
    var remark;
    if (typeof e.summary.PaymentTerms != 'undefined' && Layout == "Scaffold") {
        remark = (e.summary.PaymentTerms != null)?e.summary.PaymentTerms.replace(/\n/g, "<br/>"):"";
    }
    else {
        remark = (e.summary.Remarks != "" && e.summary.Remarks != null && typeof e.summary.Remarks != 'undefined') ? e.summary.Remarks.replace(/\n/g, "<br/>") : "";
    }
    // bind Party details
    if (Layout == "Scaffold") {
        var Caddres = (e.summary.Address != null) ? e.summary.Address : '';
        var Cperson = (e.summary.ContactPerson != null) ? e.summary.ContactPerson : '';
        var CMobile = (e.summary.Mobile != null) ? e.summary.Mobile : '';
        var CEmail = (e.summary.Email != null) ? e.summary.Email : '';
        var CTRN = (e.summary.TRN != null) ? e.summary.TRN : '';
        var cDetais = "";
        if ((conType == "DeliveryNote" || conType == "ProForma" || conType == "SalesReturn" || conType == "SalesEntry" || conType == "Quotation" || conType == "SalesOrder")) {
            cDetais += "<tr><th style='width:28% !important;'>CUSTOMER NAME</th><td>:</td><td width='69%'>" + e.summary.PartyName + "</td><tr>";
        } else {
            cDetais += "<tr><th style='width:28% !important;'>SUPPLIER NAME</th><td>:</td><td width='69%'>" + e.summary.PartyName + "</td><tr>";
        }
        cDetais += "<tr><th style='width:28% !important;'>ADDRESS</th><td>:</td><td>" + Caddres + "</td></tr>" +
                        "<tr><th style='width:28% !important;'>MOBILE NO</th><td>:</td><td>" + CMobile + "</td></tr>";
        cDetais += CEmail != '' ? "<tr><th style='width:28% !important;'>EMAIL</th><td>:</td><td>" + CEmail + "</td></tr>" : "";

        cDetais += "<tr><th style='width:28% !important;'>TRN</th><td>:</td><td>" + CTRN + "</td></tr>";
        cDetais += "<tr><th style='width:28% !important;'>CONTACT PERSON</th><td>:</td><td>" + Cperson + "</td></tr>";

        $("#partyhead").html(cDetais);
        if ((typeof e.summary.HSCode != 'undefined') && e.summary.HSCode != "" && e.summary.HSCode != null) {
            $("#HSCodeF").text(e.summary.HSCode);
        } else {
            $("#hscode").addClass("hidden");
        }
    }
    else {
        $("#lblParty").text(e.summary.PartyName);
        $("#lblParty1").text(e.summary.PartyName);
        var details = "";

        if (e.summary.Address != null) {
            details += e.summary.Address;
        }
        if (e.summary.City != null) {
            details += e.summary.Address != null ? "<br />" + e.summary.City : e.summary.City;
        }
        else if (e.summary.State != null) {
            details += details != "" ? "<br />" + e.summary.State : e.summary.State;
        }
        else if (e.summary.Country != null) {
            details += details != "" ? "<br />" + e.summary.Country : e.summary.Country;
        }
        else if (e.summary.Zip != null) {
            details += details != "" ? "<br />" + e.summary.Zip : e.summary.Zip;
        }
        details += " <br /> Phone : ";
        if (e.summary.Mobile != null) {
            details += e.summary.Mobile;
            if (e.summary.Phone != null) {
                details += ", " + e.summary.Phone;
            }
        }
        else {
            if (e.summary.Phone != null) {
                details += e.summary.Phone;
            }
        }
        if (e.summary.Email) {
            details += "<br/> Email : " + e.summary.Email
        }
        $("#lbladdress").html(details);
        $("#lbltrn").text(e.summary.TRN);
    }

    var str2 = "";
    var count = 2;
    var str1 = "";
    var str3 = "";

    // bind items
    var itemsData = bindItem(e, dvitem);
    $('#itemtable tbody').html("");
    $('#itemtable').append(itemsData);
    var grt = parseFloat(e.summary.GrandTotal).toFixed(2);
    // bind total section
    var word = conNumber(grt);

    if (Layout == "General") {

        var tcdet = "";
        if (tc != "") {
            tcdet += "<tr class='border-top'><td colspan='8'><strong><u>" + Terms + " :</u></strong><br/>" +tc + "</td></tr>";
        }
        if (dvitem == "active") {
            str1 = str3 + "</tr>" + tcdet;
        }
        else {
            str1 = wordHtml + str3 + str2 + tcdet;
        }
        if (remark != "") {
            $('#attnTable').append(remark);
        }
        else {
            $('#attnTable').hide();
        }
    }
    else if (Layout == "Scaffold") {
        var amttable = "<tr class='border-top'><tdstyle='padding: 0px;'><table class='table table-bordered' style='width:100%;border:1px solid #000'>" +
                       "<tr class='border-top'><td colspan='5'></td><td class='text-right'>Total w/o VAT </td><td class='text-center' style='width:12%;'>" + parseFloat(e.summary.SubTotal).toFixed(2) + "</td></tr>";

        if (e.summary.Discount > 0) {
            amttable += "<tr class='border-top'><td colspan='5'></td><td class='text-right'>Discount </td><td class='text-center'>" + parseFloat(e.summary.Discount).toFixed(2) + "</td></tr>";
        }
        if (e.summary.TaxAmount > 0) {
            amttable += "<tr class='border-top'><td colspan='5'></td><td class='text-right'>VAT 5% </td><td class='text-center'>" + parseFloat(e.summary.TaxAmount).toFixed(2) + "</td></tr>";
        }
        if (type != "nobillsundry") {
            amttable += bindSundry(e);
            if (e.billsundry.length > 0) {
                count += e.billsundry.length;
            }
        }
        amttable += "<tr class='border-top'><td colspan='5'><strong>UAE Dirham " + word + " Only</strong></td><td class='text-center'><b>Net Total</b></td><td class='text-center'><b>" + parseFloat(e.summary.GrandTotal).toFixed(2) + "</b></td></tr></table></td></tr>";
        str1 = amttable;
        var SaleType = (typeof e.summary.SaleType == 'undefined') ? "" : e.summary.SaleType;
        //console.log("Sale type: " + SaleType);
        var Subject = "Subject : ";
        if (SaleType == "1") {
            Subject += '<b class="text-green" style="font-size: large;font-weight: 600;">SALE</b>';
        }
        else if (SaleType == "2") {
            var From = convertToDate(e.summary.FromDate);
            var To = convertToDate(e.summary.ToDate);
            //var diff = moment.preciseDiff(e.summary.FromDate, e.summary.ToDate);
            var diff;
            var startDate = moment(From, "DD.MM.YYYY").add(1, 'days');
            var endDate = moment(To, "DD.MM.YYYY");
            var HireType = e.summary.HireType;
            var Htype = (HireType == "Weekly") ? 'week' : (HireType == "Monthly") ? 'month' : 'days';
            var HtypeV = (HireType == "Weekly") ? 'Week' : (HireType == "Monthly") ? 'Month' : 'Days';
            if (Htype == "days") {
                diff = endDate.diff(startDate, Htype);
            }
            else if (Htype == "week") {
                diff = tocountweek(endDate, startDate);
            }
            else {
                diff = tocountmonth(endDate, startDate);
            }
            $.each(e.ConvExtList, function (i, item) {
                $('#lblExtfrom').text("Extended Invoices:");
                var str = "";
                str += item.BillNo + ', ';
                $('#lblfrom').append(str);
            });


            //console.log(diff);
            Subject += '<b>HIRE OF ALUMINIUM/STEEL SCAFFOLDING FOR ' + diff + ' ' + HtypeV + '(STARTING FROM ' + From + ' TO ' + To + ' ) </b>';
        } else {
            $('#HSubject').hide();
        }
        $('#Subject').append(Subject);
        $("#hRemarks").text(remark);
        if (tc != "") {
            var Terms_C = "<tr style='border:1px solid;'><td><p style='text-align:left;text-decoration: underline;font-weight: 800;margin-bottom: 5px;'>Terms & Conditions</p><div style='padding-left:5px;'>" + tc + "</div></td></tr>";
            $('#termstable').append(Terms_C);
            $('#termstable').removeClass("hidden");
        }
    }
    else if (Layout == "Jewellery") {
        count = 1;
        if (e.summary.Discount > 0) {
            str2 += "<tr class='border-top'><td>Discount</td><td class='text-right'>" + parseFloat(e.summary.Discount).toFixed(2) + "</td></tr> ";
            count++;
            //str2 += "<tr class='border-top'><td>" + Bill_Tax + "</td><td  class='text-right'>" + parseFloat(e.summary.TaxAmount).toFixed(2) + "</td></tr>";
        }
        else {
            //str2 += "<td>VAT<span style='direction:ltr'>(5.00%)</span> برميل </td><td class='text-right'>" + parseFloat(e.summary.TaxAmount).toFixed(2) + "</td></tr>";
            // str2 += "<td>" + Bill_Tax + "</td><td class='text-right'>" + parseFloat(e.summary.TaxAmount).toFixed(2) + "</td></tr>";
        }
        if (type != "nobillsundry") {
            // bind bill sundry
            str2 += bindSundry(e);
            if (e.billsundry.length > 0) {
                count += e.billsundry.length;
            }
        }
        var MpayTable = "<table class='table table-bordered' style='width:100%;border:1px solid #000'><tr class='text-center'><th rowspan='2' class='text-center'>Mode of Payment<br>عفدا ةقيرط</th><th class='text-center' rowspan='2'>Curr</th><td colspan='2'><b>Amount المبلغ الجمالى</b></td></tr><tr class='border-top text-center'><td>FC</td><td>LC</td></tr>" +
        "<tr class='text-center border-top'><td class='text-center'>" + e.summary.paytype + " <br/> <b>Receipt Total</b></td><td>" + e.summary.Currency + "</td><td>" + parseFloat(e.summary.FCTotal).toFixed(2) + "</td><td>" + parseFloat(e.summary.GrandTotal).toFixed(2) + " </br><b>" + parseFloat(e.summary.GrandTotal).toFixed(2) + "</b></td></table>";
        var TotalTable = "<table class='table table-bordered' style='width:100%;border:1px solid #000'><tr class='text-center'><th class='text-center'>Total VATةبيرضلا عومجم </th><th class='text-right'>" + parseFloat(e.summary.TaxAmount).toFixed(2) + "</th></tr><tr><td class='text-center'>Sub Total</td><td class='text-right'>" + parseFloat(e.summary.SubTotal).toFixed(2) + "</td></tr>" + str2 +
            "<tr><td style='border-bottom: 1px solid !important;border-top: 1px solid !important;'><b>Net Total ةيلامجالا ةميقلا </b></td><td class='text-right' style='border-bottom: 1px solid !important;border-top: 1px solid !important;'><b>" + parseFloat(e.summary.GrandTotal).toFixed(2) + "</b></td></tr></table>";
        var wordHtml = '<tr class="text-center border-top"><td class="no-padding" style="width: 65%;padding-right: 5% !important;">' + MpayTable + '</td><td class="no-padding">' + TotalTable + '</td></tr>';
        var nettotal = '<tr style="border:0px;"><td class="noborder" colspan="2"><strong>' + word + ' Only</strong></td></tr>';

        var finaltotal = '<tr class="border-top"><td colspan="2" style="border-right:0px !important;"><b> Total Taxable Amount</b> </td><td colspan="4" style="border-left:0px !important;border-right:0px !important;"><b> ةبيرضلل عضاخلا غلبملا يلامجإ</b></td><td style="border-left:0px !important;" class="text-right"><b>' + parseFloat(e.summary.SubTotal).toFixed(2) + '</b></td></tr>' +
            '<tr class="border-top"><td colspan="2" style="border-right:0px !important;"><b>Total VAT </b></td><td colspan="4" style="border-left:0px !important;border-right:0px !important;"><b> ةبيرضلا عومجم</b></td><td style="border-left:0px !important;" class="text-right"><b>' + parseFloat(e.summary.TaxAmount).toFixed(2) + '</b></td></tr>' +
            '<tr class="border-top"><td colspan="2" style="border-right:0px !important;"><b>TOTAL</b></td><td colspan="4" style="border-left:0px !important;border-right:0px !important;"><b> عومجملا </b></td><td style="border-left:0px !important;" class="text-right"><b>' + parseFloat(e.summary.GrandTotal).toFixed(2) + '</b></td></tr>';

        var remarks = "";
        if (remark != "") {
            remarks = "<tr class='border-top'><td colspan='8'><strong>Remarks </strong><br /> " + remark + "</td></tr>";
        }
        str1 = wordHtml + nettotal;
        $('#itemtable2').append(finaltotal);
    }
    else if (Layout == "DotMatrix") {
        var myremark = (e.summary.Remarks != "" && e.summary.Remarks != null && typeof e.summary.Remarks != 'undefined') ? e.summary.Remarks.replace(/\n/g, "<br/>") : "";
        $("#lblattn").text(myremark);
        if (e.summary.Phone != null) {
            $("#lbltel").text(e.summary.Phone);
        }
        if (typeof e.summary.PaymentTerms != 'undefined') {
            var lblOdvn = (e.summary.PaymentTerms != null) ? e.summary.PaymentTerms : "";
            $("#lblOdvn").text(lblOdvn);
        }
        if (typeof e.summary.HSCode != 'undefined') {
            var lblYdvn = (e.summary.HSCode != null) ? e.summary.HSCode : "";
            $("#lblYdvn").text(lblYdvn);
        }

        if (typeof e.summary.Location != 'undefined') {
            var lblVehicle = (e.summary.Location != null) ? e.summary.Location : "";
            $("#lblVehicle").text(lblVehicle);
        }
        $("#lbladdresss").text(e.summary.Address);
    }
    else {
        if (e.summary.Discount > 0) {
            str2 += "<td>" + Bill_Discount + "</td><td id='discountprint' class='text-right'>" + parseFloat(e.summary.Discount).toFixed(2) + "</td></tr> ";
            count++;
            str2 += "<tr class='border-top'><td>" + Bill_Tax + "</td><td  class='text-right'>" + parseFloat(e.summary.TaxAmount).toFixed(2) + "</td></tr>";
        }
        else {
            //str2 += "<td>VAT<span style='direction:ltr'>(5.00%)</span> برميل </td><td class='text-right'>" + parseFloat(e.summary.TaxAmount).toFixed(2) + "</td></tr>";
            str2 += "<td>" + Bill_Tax + "</td><td class='text-right'>" + parseFloat(e.summary.TaxAmount).toFixed(2) + "</td></tr>";
        }
        if (type != "nobillsundry") {
            // bind bill sundry
            str2 += bindSundry(e);
            if (e.billsundry.length > 0) {
                count += e.billsundry.length;
            }
        }
        str2 += "<tr class='border-top'><th>" + Bill_Total + "</th><th class='text-right'>" + grt + "</th></tr>";

        var wordHtml = "<tr class='border-top'><td colspan='6'><strong>" + word + " Only</strong></td><td>" + Bill_Amount + "</td><td class='text-right'>" + parseFloat(e.summary.SubTotal).toFixed(2) + "</td></tr>";
        str3 = "<tr class='border-top'><td colspan='6' rowspan='" + count + "'><strong><u>" + Terms + " :</u></strong><br/>" + tc + " </td>";

        var remarks = "";
        if (remark != "") {
            remarks = "<tr class='border-top'><td colspan='8'><strong>Remarks </strong><br /> " + remark + "</td></tr>";
        }
        if (dvitem == "active") {
            str1 = str3 + "</tr>" + remarks;
        }
        else {
            str1 = wordHtml + str3 + str2 + remarks;
        }
    }
    $('#itemtable1').append(str1);
    if ($('#showzeal').prop('checked') == true) {
        $('#zeal').show();
    }
    else {
        $('#zeal').hide();
    }
    var originalpage = document.body.innerHTML;
    var printContent = $('#printit').html();
    $('body').html(printContent);

    if (e.fmapp != null) {
        $.each(e.fmapp, function (i, mapp) {

            if (mapp.Field == "Ref1") {
                $("#IblRef1").text(mapp.FieldName);
                $("#IblRef1Val").text(e.summary.Ref1);
                $("#divRef1").show();
            }
            if (mapp.Field == "Ref2") {
                $("#IblRef2").text(mapp.FieldName);
                $("#IblRef2Val").text(e.summary.Ref2);
                $("#divRef2").show();
            }
            if (mapp.Field == "Ref3") {
                $("#IblRef3").text(mapp.FieldName);
                $("#IblRef3Val").text(e.summary.Ref3);
                $("#divRef3").show();
            }
            if (mapp.Field == "Ref4") {
                $("#IblRef4").text(mapp.FieldName);
                $("#IblRef4Val").text(e.summary.Ref4);
                $("#divRef4").show();
            }
            if (mapp.Field == "Ref5") {
                $("#IblRef5").text(mapp.FieldName);
                $("#IblRef5Val").text(e.summary.Ref5);
                $("#divRef5").show();
            }
        });
    }


    //$('title').html(e.summary.BillNo);
    // find height

    switch (conType) {
        case 'PurchaseOrder':
            var titname = "Purchase Order - " + e.summary.PartyName + " - " + e.summary.BillNo;
            $('title').html(titname);
            break;
        case 'PurchaseReturn':
            var titname = "Purchase Return - " + e.summary.PartyName + " - " + e.summary.BillNo;
            $('title').html(titname);
            break;
        case 'PurchaseEntry':
            var titname = "Purchase Entry - " + e.summary.PartyName + " - " + e.summary.BillNo;
            $('title').html(titname);
            break;
        case 'DeliveryNote':
            var titname = "Delivery Note - " + e.summary.PartyName + " - " + e.summary.BillNo;
            $('title').html(titname);
            break;
        case 'ProForma':
            var titname = "Pro Forma - " + e.summary.PartyName + " - " + e.summary.BillNo;
            $('title').html(titname);
            break;
        case 'SalesReturn':
            var titname = "Sales Return - " + e.summary.PartyName + " - " + e.summary.BillNo;
            $('title').html(titname);
            break;
        case 'SalesEntry':
            var titname = "Tax Invoice - " + e.summary.PartyName + " - " + e.summary.BillNo;
            $('title').html(titname);
            break;
        case 'Quotation':
            var titname = "Quotation - " + e.summary.PartyName + " - " + e.summary.BillNo;
            $('title').html(titname);
            break;
        case 'SalesOrder':
            var titname = "Sales Order - " + e.summary.PartyName + " - " + e.summary.BillNo;
            $('title').html(titname);
            break;
        default:
            var titname = "Tax Invoice - " + e.summary.PartyName + " - " + e.summary.BillNo;
            $('title').html(titname);
            break;
    }

    $('title').html(titname);

    var header = $(".print thead").height(); // default 265
    var items = $("#itemSection").height(); // default 558
    var itemstable = $("#itemtable").height();
    var terms = $("#itemtable1").height(); // default 137
    var footer = $("#footer").height(); // default 50
    var height = $(".print").height(); // total 
    if (terms > 137 && itemstable < 558) {
    }
    if (Layout == "Jewellery") {
        if (itemstable < 500) {
            var trheight = 500 - itemstable;
            var dummytable = "<tr style='height:" + trheight + "px'><td></td><td></td><td></td><td></td><td></td><td></td><td></td></tr>";
            $('#jwltotal').before(dummytable);
        }
        var cusHeight = $(".jewel-cus").height();
        var inHeight = $(".jewel-inv").height();
        if (cusHeight < inHeight) {
            var trheight = inHeight - cusHeight;
            var dummytable = "<tr style='height:" + trheight + "px'><td colspan='3'></td></tr>";
            $('.jewel-cus').append(dummytable);
        }
        if (cusHeight > inHeight) {
            var trheight = cusHeight - inHeight;
            var dummytable = "<tr style='height:" + trheight + "px'><td colspan='2'></td></tr>";
            $('.jewel-inv').append(dummytable);
        }
    }
    if (Layout == "General") {
        if (itemstable < 359) {
            var trheight = 359 - itemstable;
            var dummytable = "<tr class='noborder' style='height:" + trheight + "px'><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td></tr>";
            $('#gentotal').before(dummytable);
        }
    }
    if (Layout == "Default") {
        if (itemstable < 510) {
            var trheight = 410- itemstable;
            var dummytable = "<tr class='noborder' style='height:" + trheight + "px'><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td></tr>";
            $('#itemtable tbody').append(dummytable);
        }
    }
    // console.log("cusHeight - " + cusHeight + "inHeight - " + inHeight + "Header - " + header + "\n items -" + items + "\n itemstable - " + itemstable + "\n terms-" + terms + "\n footer -" + footer + "\n full height - " + height);
   window.scrollTo(0, 0);
    setTimeout(function () { window.print(); }, e.summary.TimeOut);
}

function printiteminvoice(e) {
    var total = parseFloat(0);
    $.each(e.item, function (i, item) {
        var subtot = parseFloat(item.ItemTotalAmount.toFixed(2));
        total += subtot;
        var str = '<tr>';
        str += '<td>' + item.ItemCode + "-" + item.ItemName + '</td>';
        str += '<td>' + item.ItemUnit + '</td>';
        str += '<td>' + item.ItemQuantity + '</td>';
        str += '<td class="text-right">' + parseFloat(item.ItemUnitPrice).toFixed(2) + '</td>';
        str += '<td class="text-right">' + parseFloat(item.ItemSubTotal).toFixed(2) + '</td>';
        str += '<td class="text-right">' + parseFloat(item.ItemTax).toFixed(2) + "%" + '</td>';
        str += '<td class="text-right">' + parseFloat(item.ItemTaxAmount).toFixed(2) + '</td>';
        str += '<td>' + subtot + '</td>';
        str += '</tr>';
        $('#itemtable').append(str);
    });

    $.each(e.billsundry, function (i, billsundry) {
        var type = "";
        var type2 = "";
        var symbol = "";
        var value = "";
        if (billsundry.BsType == 0) {
            type = "Add";
        } else {
            type = "Less";
        }
        if (billsundry.AmountType == 1) {
            type2 = "&#64;";
            symbol = "%";
        } else {
            type2 = "";
            symbol = "";
        }
        if (billsundry.BsValue > 0) {
            value = parseFloat(billsundry.BsValue).toFixed(2);
        } else {
            value = "";
        }

        var str = "<tr class='border-top'>";
        str += '<td colspan="6"></td><td>' + billsundry.BillSundry + '</td>';
        //str += '<td>' + type2 + value + '</td>';
        //str += '<td>' + symbol + '</td>';
        str += '<td class="text-right">' + parseFloat(billsundry.BsAmount).toFixed(2) + '</td>';
        str += '</tr>';
        $('#itemtable1').append(str);
    });
    return true;
}
// function to print invoice in pos format
function PrintPOSInvoice(e, type) {

    //sale
    var summary = {};
    var item = {};
    var billsundry = {};
    var trtype = "";
    var netAmt = "";
    var netPayRec = "";
    summary = e.summary;
    item = e.item;
    billsundry = e.billsundry;
    PosSalePrint(summary, item, billsundry, trtype, type);
    $("#PosReturn").hide();
    $("#PayRecNetAmt").hide();

    //sale return
    if ((typeof e.summaryR != 'undefined') && e.summaryR != null && e.summaryR != "") {
        summary = e.summaryR;
        item = e.itemR;
        billsundry = e.billsundryR;
        trtype = "R";
        netAmt = e.summaryR.GrandTotal > e.summary.GrandTotal ? (e.summaryR.GrandTotal - e.summary.GrandTotal) : (e.summary.GrandTotal - e.summaryR.GrandTotal);
        netPayRec = e.summaryR.GrandTotal > e.summary.GrandTotal ? "Payable" : "Receivable";
        netPayRec = netAmt == 0 ? "Receivable" : netPayRec;

        $("#PosReturn").show();
        $("#PayRecNetAmt").show();

        $("#lblPayRec").text("Net " + netPayRec);
        $("#lblPayRecAmt").text(parseFloat(netAmt).toFixed(2));

        PosSalePrint(summary, item, billsundry, trtype, type);
        $("#PosReturn").show();
    }


    var originalpage = document.body.innerHTML;
    var printContent = $('#printit').html();
    $('body').html(printContent);
    $('title').html(e.summary.BillNo);
    $('#backBtn').attr('href', window.location.href);
    window.print();
}
function PosSalePrint(summary, item, billsundry, trtype, type) {
    $("#lblBillNo" + trtype).text(summary.BillNo);
    $("#lblDate" + trtype).text(convertToDate(summary.Date));
    $("#lblEmployee" + trtype).text(summary.Cashier);
    $("#lblpaytype" + trtype).text(summary.paytype);


    if (summary.Location != null) {
        $("#lblLocation" + trtype).text(summary.Location);
    }


    if (type == "sales") {
        $("#lblPONo" + trtype).text(summary.PONo);
    }
    if (type != "Addtax") {
        $('.addtax').remove();
    }

    if (summary.ConvertType != null && summary.ConvertNo != null) {
        $("#lblConvertType" + trtype).text(summary.ConvertType);
        $("#lblConvertNo" + trtype).text(summary.ConvertNo);
        $("#tranConvert" + trtype).show();
    } else {
        $("#tranConvert" + trtype).hide();
    }

    var details = summary.PartyName;
    if (summary.Address != null) {
        details += "<br />" + summary.Address;
    }
    if (summary.City != null) {
        details += "<br />" + summary.City;
    }
    else if (summary.State != null) {
        details += "<br />" + summary.State;
    }
    else if (summary.Country != null) {
        details += "<br/>" + summary.Country;
    }
    else if (summary.Zip != null) {
        details += "<br />" + summary.Zip;
    }
    details += " <br/> Phone : ";
    if (summary.Mobile != null) {
        details += summary.Mobile;
        if (summary.Phone != null) {
            details += ", " + summary.Phone;
        }
    }
    else {
        if (summary.Phone != null) {
            details += summary.Phone;
        }
    }
    if (summary.Email) {
        details += "<br/> Email : " + summary.Email
    }
    if (summary.TRN) {
        details += "<br/> TRN : " + summary.TRN
    }
    $("#lblCustomer" + trtype).html(details);
    var remark = summary.Note.replace(/\n/g, "<br/>");
    $("#lblTC" + trtype).html(remark);
    // bind items
    var itemsData = bindPOSItem(item, summary, type);
    $('#itemtable' + trtype).append(itemsData);
    if (summary.Discount > 0) {
        $("#lblDiscAmt" + trtype).text(parseFloat(summary.Discount).toFixed(2));
    }
    else {
        $(".discpt").hide();
    }
    $("#lblTax" + trtype).text(parseFloat(summary.TaxAmount).toFixed(2));
    $("#lblAmount" + trtype).text(parseFloat(summary.SubTotal).toFixed(2));
    var str = "";

    var spanval = type == "POSInvoice1" ? 4 : 3;
    var arabic = type == "POSInvoice1" ? "" : "المبلغ الإجمالي";

    $.each(billsundry, function (i, billsundry) {
        str += "<tr class='tabletitle'>";
        str += '<td colspan="' + spanval + '" class="Rate"><h2><strong>' + billsundry.BillSundry + '</strong></h2></td>';
        str += '<td colspan="3" class="rate"><h2>' + parseFloat(billsundry.BsAmount).toFixed(2) + '</h2></td>';
        str += '</tr>';
    });
    str += '<tr class="tabletitle"><td colspan="' + spanval + '" class="Rate"><h2><strong>Grand Total ' + arabic + '(AED)</strong></h2></td>' +
    '<td colspan="3" class="rate"><h2>' + parseFloat(summary.GrandTotal).toFixed(2) + '</h2></td></tr>';

    $('#posfoot' + trtype).append(str);

    //var originalpage = document.body.innerHTML;
    //var printContent = $('#printit').html();
    //$('body').html(printContent);
    //$('title').html(e.summary.BillNo);
    //$('#backBtn').attr('href', window.location.href);
    //window.print();
}

//itembind for POS
function bindPOSItem(item, summary, type) {
    var total = parseFloat(0);
    var str = "";
    var itemcode = "";
    var count = 1;

    if (type == "POSInvoice1") {

        $.each(item, function (i, item) {
            var subtot = parseFloat(item.ItemTotalAmount.toFixed(2));
            total += subtot;
            var itemnote = "";
            if (item.ItemNote != "" && (typeof item.ItemNote != 'undefined')) {
                itemnote = "<br /><small>" + item.ItemNote + "</small>";
            }
            if (item.bundle != null && item.bundle.length > 0) {
                var desc = "<br/>[<span class='descr' data-name='itemNote'>";
                $.each(item.bundle, function (j, itemss) {
                    desc += itemss.ItemCode + " - " + itemss.ItemName;
                    desc += " - " + parseFloat(itemss.quantity).toFixed(2) + " ";
                    desc += (itemss.ItemUnitName != null) ? itemss.ItemUnitName : "";
                    desc += "<br/>";
                });
                desc += "</span>]";
                itemnote = itemnote + desc;
            }


            //itemcode
            if (summary.chkCode == 0) {
                itemcode = item.ItemCode;
            }
            var unit = (item.ItemUnit != null) ? item.ItemUnit : "";
            str += '<tr>';
            str += '<td class="text-left">' + count + '</td>';
            str += '<td class="text-left">' + itemcode + '</td>';
            str += '<td class="text-center">' + parseFloat(item.ItemUnitPrice).toFixed(2) + '</td>';
            str += '<td class="text-center">' + parseFloat(item.ItemQuantity).toFixed(2) + '</td>';
            str += '<td class="text-center">' + parseFloat(item.ItemDiscount).toFixed(2) + '</td>';
            str += '<td class="text-center">' + parseFloat(item.ItemTaxAmount).toFixed(2) + '</td>';
            str += '<td class="text-right">' + parseFloat(item.ItemSubTotal).toFixed(2) + '</td>';
            str += '</tr>';
            str += '<tr>';
            str += '<td></td>';
            str += '<td class="text-left" colspan="6">' + item.ItemName + itemnote + '-' + unit + '</td>';
            str += '</tr>';
            count++;
        });

    } else {
        $.each(item, function (i, item) {
            var subtot = parseFloat(item.ItemTotalAmount.toFixed(2));
            total += subtot;
            var itemnote = "";
            if (item.ItemNote != "" && (typeof item.ItemNote != 'undefined')) {
                itemnote = "<br /><small>" + item.ItemNote + "</small>";
            }
            if (item.bundle != null && item.bundle.length > 0) {
                var desc = "<br/>[<span class='descr' data-name='itemNote'>";
                $.each(item.bundle, function (j, itemss) {
                    desc += itemss.ItemCode + " - " + itemss.ItemName;
                    desc += " - " + parseFloat(itemss.quantity).toFixed(2) + " ";
                    desc += (itemss.ItemUnitName != null) ? itemss.ItemUnitName : "";
                    desc += "<br/>";
                });
                desc += "</span>]";
                itemnote = itemnote + desc;
            }


            //itemcode
            if (summary.chkCode == 0) {
                itemcode = item.ItemCode + " - ";
            }
            var unit = (item.ItemUnit != null) ? item.ItemUnit : "";
            str += '<tr>';
            str += '<td>' + itemcode + item.ItemName + ' ' + unit + itemnote + '</td>';
            str += '<td class="text-center">' + item.ItemQuantity + '</td>';
            if (type == "POStax") {
                var itemtax = item.ItemTax != 0 ? parseFloat(item.ItemUnitPrice) * (parseFloat(item.ItemTax) / 100) : 0;
                var itemprice = parseFloat(item.ItemUnitPrice) + parseFloat(itemtax)
                str += '<td class="text-center">' + parseFloat(itemprice).toFixed(2) + '</td>';
                str += '<td class="text-right">' + subtot.toFixed(2) + '</td>';

            } else if (type == "Addtax") {
                str += '<td class="text-center">' + parseFloat(item.ItemUnitPrice).toFixed(2) + '</td>';
                str += '<td class="text-right">' + parseFloat(item.ItemSubTotal).toFixed(2) + '</td>';
                str += '<td class="text-right">' + parseFloat(item.ItemTaxAmount).toFixed(2) + '</td>';
                str += '<td class="text-right">' + subtot + '</td>';

            }
            else {
                str += '<td class="text-center">' + parseFloat(item.ItemUnitPrice).toFixed(2) + '</td>';
                str += '<td class="text-right">' + parseFloat(item.ItemSubTotal).toFixed(2) + '</td>';
            }
            //str += '<td class="text-right">' + parseFloat(item.ItemTaxAmount).toFixed(2) + '</td>';
            //str += '<td class="text-right">' + subtot.toFixed(2) + '</td>';
            str += '</tr>';
            count++;
        });
    }


    return str;
}
//change cofactor
function coFactorChange() {
    if ($("#ItemUnitID").prop('selectedIndex') > 0 && $("#SubUnitId").prop('selectedIndex') > 0) {
        $('#ConFactor').prop('readonly', false);

        var pry = $("#ItemUnitID").val();
        var sec = $("#SubUnitId").val();

        if (pry == sec) {
            $("#ConFactor").val(1);
            $('#ConFactor').prop('readonly', true);
        } else {
            $('#ConFactor').prop('readonly', false);
        }
    } else {
        $('#ConFactor').prop('readonly', true);
    }
}
//sales exec
function salesExecPopUp() {

    $(".salesexec").select2({
        placeholder: 'Search Sales Person by Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/Employee/SearchEmployee",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page
                };
            },
            processResults: function (data, params) {
                params.page = params.page || 1;

                return {
                    results: data
                };
            },
            cache: true
        },
    });
}




//----------------------------------------------------------
//sales person select2 in customer/supplier pop up
$.fn.modal.Constructor.prototype.enforceFocus = function () { };
$('#modal-create').on('shown.bs.modal', function (e) {

    $(".salesexec").select2({
        placeholder: 'Search Sales Person by Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/Employee/SearchEmployee",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "empty"
                };
            },
            processResults: function (data, params) {
                params.page = params.page || 1;

                return {
                    results: data
                };
            },
            cache: true
        },
    });

    $(".accid").select2({
        placeholder: 'Search Account By Code or name',
        minimumInputLength: 0,
        ajax: {
            url: "/Accounts/AllBankAccounts",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page
                };
            },
            processResults: function (data, params) {
                params.page = params.page || 1;

                return {
                    results: data
                };
            },
            cache: true
        },
    });

});
$('.salesexec').select2({
    dropdownParent: $('#modal-create')
});
//---------------------------------------------------

//customer mailid 
function GetCustomerMail() {
    var CustomerId = $('#ddlCustomer').val();
    if (CustomerId != null) {
        $.ajax({
            url: '/Customer/GetCustomerEmailById',
            type: "GET",
            dataType: "JSON",
            data: { CustID: CustomerId },
            success: function (result) {
                $("#custEmailId").val(result.EmailId);
            }
        });

        var cnt = $('#addinvoiceItem tr').length;
        if ($('#MuDvNt').length) {
            $('#MuDvNt').show();
            //$("#addinvoiceItem").empty();
            //alert($('#InvoiceId').text());
            if ($('#InvoiceId').text() != "") {
                $("#addinvoiceItem").html("");
                addrow('addinvoiceItem', 'sales', "", "0.00", "0.00", "0");
            }
        }
    }
}
function GetSupplierMail() {
    var SuppId = $('#ddlSupplier').val();
    if (SuppId != null) {
        $.ajax({
            url: '/Supplier/GetSupplierEmailById',
            type: "GET",
            dataType: "JSON",
            data: { SuppId: SuppId },
            success: function (result) {
                $("#suppEmailId").val(result.EmailId);
            }
        });
    }
}

//item pop up
function AddItemPopUp() {
    /* function for Create popup for item large */
    $('table').on('click', '.modal-create-lg', function (e) {
        e.preventDefault();
        var url = $(this).attr('href');
        modalshow(url, '#modal-create-lg');
        //$('#KeepStock').prop('checked', true);
        //$('#StockSection').show();
        //$("#SubUnitId").rules("remove", "required");
    });


    //primary unit change
    $(document).on('change', '#ItemUnitID', function (event) {
        if ($("#ItemUnitID").prop('selectedIndex') > 0) {
            $('#SubUnitId').prop('disabled', false);
            var textvalue = $("#ItemUnitID option:selected").text();
            $("#PrUnit1").text(textvalue);
            $("#PrUnit2").text(textvalue);
        } else {
            $("#SubUnitId").prop('selectedIndex', 0);
            $('#SubUnitId').prop('disabled', true);
            $("#PrUnit1").text("");
            $("#PrUnit2").text("");
        }
        coFactorChange();
    });
    //sec unit change
    $(document).on('change', '#SubUnitId', function (event) {
        coFactorChange();
    });


    //$('#KeepStock').click(function () {

    //    if ($('#KeepStock').prop('checked') == true) {
    //        $('#StockSection').show();
    //    }
    //    else {
    //        $('#StockSection').hide();
    //    }
    //});





    //$('#modal-create-lg').on('submit', '#createitemform', function (e) {
    //    e.preventDefault();
    //    var url = $('#createitemform')[0].action;
    //    var formData = new FormData(this);
    //    $.ajax({
    //        type: "POST",
    //        url: url,
    //        data: formData,
    //        processData: false,
    //        contentType: false,
    //        success: function (data) {
    //            if (data.status) {
    //                $('#modal-create-lg').modal('hide');
    //                $('.ajax_response', res_success).text(data.message);
    //                $('.AlertDiv').prepend(res_success);
    //            }
    //            else {
    //                $('.ajax_response', res_danger).text(data.message);
    //                $('.AlertDiv').prepend(res_danger);
    //            }
    //            fadeAlert();
    //        }
    //        ,
    //        error: function (jqXHR, textStatus, errorThrown) {
    //            alert("error");
    //        }
    //    })
    //    //// var data = new FormData(this);
    //    //$('#createitemform').serialize();
    //    //createajax(url, data, '#modal-create-lg');
    //});

    $('body').on('click', '.modal-close-btn', function () {
        $('#modal-create-lg').modal('hide');
        $('#modal-create-lg').removeData('bs.modal');
    });
    $('#modal-create-lg').on('hidden.bs.modal', function () {
        $(this).removeData('bs.modal');
    });





    $('div').on('click', '.printPopUp', function (e) {
        e.preventDefault();
        $('#modal-container-barcode').modal('show');
        var openStock = parseInt($("#OpeningStock").val());
        $("#labelCount").val(openStock);
        //$(this).attr('data-target', '#modal-container-brand');
        //$(this).attr('data-toggle', 'modal');
    });

    //submit function
    $('#modal-create-lg').on('submit', '#createitemform', function (e) {
        //$('#modal-container-barcode').on('submit', '#createitemform', function (e) {
        e.preventDefault();
        var url = $('#createitemform')[0].action;
        var formData = new FormData(this);

        var imgUpload = $("#ItemImage").get(0);
        var imgFiles = imgUpload.files;
        if (imgFiles[0] != null) {
            formData.append(imgFiles[0].name, imgFiles[0]);
        }
        var docUpload = $("#ItemDocument").get(0);
        var docFiles = docUpload.files;
        if (docFiles[0] != null) {
            formData.append(docFiles[0].name, docFiles[0]);
        }

        var fnval = $('input[type="submit"], button[type="submit"]', this).filter(':focus').attr('id');

        var pcont = $("#printcount").val();
        formData.append("fnval", fnval);
        formData.append("printcount", pcont);

        $.ajax({
            async: true,
            cache: false,
            dataType: "json",
            type: "POST",
            processData: false,
            contentType: false,
            url: url,
            data: formData,//JSON.stringify(parameters),
            beforeSend: function () {
                $("button").prop('disabled', true); // disable button
            },
            success: function (e) {
                if (e.status) {
                    if (fnval == "print") {
                        PrintBarcode(e);
                    } else {
                        $('.ajax_response', res_success).text(e.message);
                        $('.AlertDiv').prepend(res_success);
                        //   window.location.href = '@Url.Action("Create", "Item")';
                    }
                    $('#modal-create-lg').modal('hide');
                    $('#modal-create-lg').removeData('bs.modal');

                    $('#modal-container-barcode').modal('hide');
                    $('#modal-container-barcode').removeData('bs.modal');

                }
                else {
                    $('.ajax_response', res_danger).text(e.message);
                    $('.AlertDiv').prepend(res_danger);
                }
                fadeAlert();
                $("button").prop('disabled', false);
            }
        });


        function PrintBarcode(e) {
            var count = e.item.PCount;
            var itemName = e.item.ItemName;
            var itemPrice = e.item.ItemPrice;
            var barCode = e.item.Barcode;

            printSticker('barcode', barCode);
            var image = $("#cont").html();
            var CName = $("#cname").val();

            var i = 0;
            var table = "";
            var str = '<h4>' + CName + '</h4><div>' + image + '</div><p style="margin-top: -12px;line-height: 10px;position: relative;">' + itemName + '<br> DHS    ' + itemPrice + '</p>';
            while (i < count) {
                table += "<tr><td> " + str + "</td></tr>";
                i++;
            }
            $('#bartable').append(table);
            //var originalpage = document.body.innerHTML;
            //var printContent = $('#printitbr').html();
            //$('body').html(printContent);
            //$('title').html("Barcode Print");
            // window.print();

            var divToPrint = $("#printBarcode").html();
            var newWin = window.open('', 'Print-Window');
            newWin.document.open();
            newWin.document.write('<html><body onload="window.print()">' + divToPrint + '</body></html>');
            newWin.document.close();

            setTimeout(function () { newWin.close(); }, e.summary.TimeOut);

            // window.location.href = '@Url.Action("Create", "Item")';
        }


    });





    $('div').on('click', '.btncategoryAdd', function (e) {
        e.preventDefault();
        $(this).attr('data-target', '#modal-container-category');
        $(this).attr('data-toggle', 'modal');
    });
    $('div').on('click', '.btncolorAdd', function (e) {
        e.preventDefault();
        $(this).attr('data-target', '#modal-container-color');
        $(this).attr('data-toggle', 'modal');
    });
    $('div').on('click', '.btntaxAdd', function (e) {
        e.preventDefault();
        $(this).attr('data-target', '#modal-container-tax');
        $(this).attr('data-toggle', 'modal');
    });
    $('div').on('click', '.btnbrandAdd', function (e) {
        e.preventDefault();
        $(this).attr('data-target', '#modal-container-brand');
        $(this).attr('data-toggle', 'modal');
    });
    $('div').on('click', '.btnsizeAdd', function (e) {
        e.preventDefault();
        $(this).attr('data-target', '#modal-container-size');
        $(this).attr('data-toggle', 'modal');
    });
    $('div').on('click', '.btnunitAdd', function (e) {
        e.preventDefault();
        $(this).attr('data-target', '#modal-container-unit');
        $(this).attr('data-toggle', 'modal');
    });


    $('#modal-container-category').on('submit', '#createform', function (e) {
        var url = $('#modal-container-category #createform')[0].action;
        var text = $("#ItemCategoryName").val();
        $('#ItemCategoryID option:selected').attr("selected", null);
        $.ajax({
            type: "POST",
            url: url,
            data: $('#modal-container-category #createform').serialize(),
            success: function (data) {
                if (data.status) {
                    $('#modal-container-category').modal('hide');

                    var newOption = $('<option></option>');
                    newOption.val(data.Id).attr("selected", "selected");
                    newOption.html(text);
                    $('.form-control[name="ItemCategoryID"]').append(newOption)
                }
                else {
                    $('.ajax_response', res_danger).text(data.message);
                    $('.AlertDiv').prepend(res_danger);
                }
                fadeAlert();
            }
        })

        e.preventDefault();
    })

    $('#modal-container-color').on('submit', '#createform', function (e) {

        var url = $('#modal-container-color #createform')[0].action;
        var text = $("#ItemColorName").val();
        $('#ItemColorID option:selected').attr("selected", null);
        $.ajax({
            type: "POST",
            url: url,
            data: $('#modal-container-color #createform').serialize(),
            success: function (data) {
                if (data.status) {
                    $('#modal-container-color').modal('hide');

                    var newOption = $('<option></option>');
                    newOption.val(data.Id).attr("selected", "selected");
                    newOption.html(text);
                    $('.form-control[name="ItemColorID"]').append(newOption)
                }
                else {
                    $('.ajax_response', res_danger).text(data.message);
                    $('.AlertDiv').prepend(res_danger);
                }
                fadeAlert();
            }
        })

        e.preventDefault();
    })
    $('#modal-container-tax').on('submit', '#createform', function (e) {
        var url = $('#modal-container-tax #createform')[0].action;
        var text = $("#ItemTaxPercentage").val();
        $('#TaxID option:selected').attr("selected", null);
        $.ajax({
            type: "POST",
            url: url,
            data: $('#modal-container-tax #createform').serialize(),
            success: function (data) {
                if (data.status) {
                    $('#modal-container-tax').modal('hide');

                    var newOption = $('<option></option>');
                    newOption.val(data.Id).attr("selected", "selected");
                    newOption.html(text);
                    $('.form-control[name="TaxID"]').append(newOption)
                }
                else {
                    $('.ajax_response', res_danger).text(data.message);
                    $('.AlertDiv').prepend(res_danger);
                }
                fadeAlert();
            }
        })

        e.preventDefault();
    })
    $('#modal-container-brand').on('submit', '#createform', function (e) {
        var url = $('#modal-container-brand #createform')[0].action;
        var text = $("#ItemBrandName").val();
        $('#ItemBrandID option:selected').attr("selected", null);
        $.ajax({
            type: "POST",
            url: url,
            data: $('#modal-container-brand #createform').serialize(),
            success: function (data) {
                if (data.status) {
                    $('#modal-container-brand').modal('hide');

                    var newOption = $('<option></option>');
                    newOption.val(data.Id).attr("selected", "selected");
                    newOption.html(text);
                    $('.form-control[name="ItemBrandID"]').append(newOption)
                }
                else {
                    $('.ajax_response', res_danger).text(data.message);
                    $('.AlertDiv').prepend(res_danger);
                }
                fadeAlert();
            }
        })

        e.preventDefault();
    })
    $('#modal-container-size').on('submit', '#createform', function (e) {
        var url = $('#modal-container-size #createform')[0].action;
        var text = $("#ItemSizeName").val();
        $('#ItemSizeID option:selected').attr("selected", null);
        $.ajax({
            type: "POST",
            url: url,
            data: $('#modal-container-size #createform').serialize(),
            success: function (data) {
                if (data.status) {
                    $('#modal-container-size').modal('hide');

                    var newOption = $('<option></option>');
                    newOption.val(data.Id).attr("selected", "selected");
                    newOption.html(text);
                    $('.form-control[name="ItemSizeID"]').append(newOption)
                }
                else {
                    $('.ajax_response', res_danger).text(data.message);
                    $('.AlertDiv').prepend(res_danger);
                }
                fadeAlert();
            }
        })

        e.preventDefault();
    })

    $('#modal-container-unit').on('submit', '#createform', function (e) {
        var url = $('#modal-container-unit #createform')[0].action;
        var text = $("#ItemUnitName").val();
        $('#ItemUnitID option:selected').attr("selected", null);
        $('#SubUnitId option:selected').attr("selected", null);
        $.ajax({
            type: "POST",
            url: url,
            data: $('#modal-container-unit #createform').serialize(),
            success: function (data) {
                if (data.status) {
                    $('#modal-container-unit').modal('hide');

                    var newOption = $('<option></option>');
                    newOption.val(data.Id).attr("selected", "selected");
                    newOption.html(text);
                    $('#ItemUnitID').append(newOption);

                    var newOptions = $('<option></option>');
                    newOptions.val(data.Id).attr("selected", "selected");
                    newOptions.html(text);
                    $('#SubUnitId').append(newOptions);
                }
                else {
                    $('.ajax_response', res_danger).text(data.message);
                    $('.AlertDiv').prepend(res_danger);
                }
                fadeAlert();
            }
        })

        e.preventDefault();
    })
    $('body').on('click', '.modal-close-btn', function () {
        $('#modal-create-lg').modal('hide');
        $('#modal-create-lg').removeData('bs.modal');
    });

    $('body').on('click', '.modal-close-btn', function () {
        $('#modal-container-brand').modal('hide');
        $('#modal-container-brand').removeData('bs.modal');
    });
    $('body').on('click', '.modal-close-btn', function () {
        $('#modal-container-category').modal('hide');
        $('#modal-container-category').removeData('bs.modal');
    });
    $('body').on('click', '.modal-close-btn', function () {
        $('#modal-container-tax').modal('hide');
        $('#modal-container-tax').removeData('bs.modal');
    });
    //clear modal cache, so that new content can be loaded;
    $('#modal-container-brand').on('hidden.bs.modal', function () {
        $(this).removeData('bs.modal');
    });
    $('#modal-container-category').on('hidden.bs.modal', function () {
        $(this).removeData('bs.modal');
    });
    $('#modal-container-tax').on('hidden.bs.modal', function () {
        $(this).removeData('bs.modal');
    });
    $('#modal-container-unit').on('hidden.bs.modal', function () {
        $(this).removeData('bs.modal');
    });
    $('#modal-container-size').on('hidden.bs.modal', function () {
        $(this).removeData('bs.modal');
    });
    $('#modal-container-color').on('hidden.bs.modal', function () {
        // $(this).removeData('bs.modal');
        $('#modal-container-color').modal('hide');
    });

    $('#CancelModal').on('click', function () {
        return false;
    });








    //department and desgn
    $('div').on('click', '.btnDept', function (e) {
        e.preventDefault();
        var url = $(this).attr('href');
        modalshow(url, '#modal-container-dept');
    });
    $('div').on('click', '.btnDegn', function (e) {
        e.preventDefault();
        var url = $(this).attr('href');
        modalshow(url, '#modal-container-degn');
    });

    $('#modal-container-dept').on('submit', '#createform', function (e) {
        var url = $('#modal-container-dept #createform')[0].action;
        var text = $("#DepartmentName").val();
        $('#DepartmentID option:selected').attr("selected", null);
        $.ajax({
            type: "POST",
            url: url,
            data: $('#modal-container-dept #createform').serialize(),
            success: function (data) {
                if (data.status) {
                    $('#modal-container-dept').modal('hide');

                    var newOption = $('<option></option>');
                    newOption.val(data.Id).attr("selected", "selected");
                    newOption.html(text);
                    $('.form-control[name="DepartmentID"]').append(newOption)
                }
                else {
                    $('.ajax_response', res_danger).text(data.message);
                    $('.AlertDiv').prepend(res_danger);
                }
                fadeAlert();
            }
        })

        e.preventDefault();
    });

    $('#modal-container-degn').on('submit', '#createform', function (e) {
        var url = $('#modal-container-degn #createform')[0].action;
        var text = $("#DesignationName").val();
        $('#DesignationID option:selected').attr("selected", null);
        $.ajax({
            type: "POST",
            url: url,
            data: $('#modal-container-degn #createform').serialize(),
            success: function (data) {
                if (data.status) {
                    $('#modal-container-degn').modal('hide');

                    var newOption = $('<option></option>');
                    newOption.val(data.Id).attr("selected", "selected");
                    newOption.html(text);
                    $('.form-control[name="DesignationID"]').append(newOption)
                }
                else {
                    $('.ajax_response', res_danger).text(data.message);
                    $('.AlertDiv').prepend(res_danger);
                }
                fadeAlert();
            }
        })

        e.preventDefault();
    });
    $('body').on('click', '.modal-close-btn', function () {
        $('#modal-container-dept').modal('hide');
        $('#modal-container-dept').removeData('bs.modal');
    });
    $('body').on('click', '.modal-close-btn', function () {
        $('#modal-container-degn').modal('hide');
        $('#modal-container-degn').removeData('bs.modal');
    });

    $('body').on('change', '#OpeningStock', function (e) {
        var Opstock = $("#OpeningStock").val();
        var PurPrice = $("#PurchasePrice").val();
        var StockVal = Opstock * PurPrice;
        $('#StockValue').val(StockVal);
    });
    $('body').on('change', '#PurchasePrice', function (e) {
        var Opstock = $("#OpeningStock").val();
        var PurPrice = $("#PurchasePrice").val();
        var StockVal = Opstock * PurPrice;
        $('#StockValue').val(StockVal);
    });
}


function setTabIndex() {
    var j = 1;
    $('body').find('input,textarea,select,button, .select2-container .select2-selection__rendered').not(".select2-hidden-accessible").not(":hidden").each(function (i) {
        if (!$(this).hasClass("select2-hidden-accessible") && !$(this).is(":hidden")) {
            $(this).attr('tabindex', j);
            j++;
        }
        if ($(this).closest("tr").hasClass("item_") && !$(this).hasClass("select2-selection__rendered")) {
            $(this).attr('tabindex', -1);
        }
    });
}

// generate Price Table
function GeneratePriceTable(fnval) {
    $("#fnvalp").val(fnval);
    var cols = [];
    $("#normalinvoice").find('tr').not(':first, .item_').each(function (rowIndex, r) {
        var iiD = $(this).find('select.item_name').val();
        if (typeof iiD !== "undefined") {
            cols.push(iiD);
        }
    });
    var items = cols.filter(function (itm, i, cols) {
        return i == cols.indexOf(itm);
    });
    $.ajax({
        url: '/Item/ItemDetails',
        dataType: 'json',
        type: "POST",
        data: { items: items },
        cache: true,
        success: function (data) {
            var ittable = "";
            $.each(data, function (i, item) {
                ittable += "<tr><td>" + (i + 1) + "</td>" +
                    "<td>" + item.ItemCode + "-" + item.ItemName + "<input type='hidden' data-name='ItemId' name='Addon[" + i + "].ItemId' class='iitemid' value='" + item.ItemID + "'/> </td>" +
                    "<td><input type='number' step=any data-name='SellingPrice' name='Addon[" + i + "].SellingPrice' id='SellingPrice_" + i + "' value='" + item.SellingPrice + "'  class='SellingPrice_" + i + " form-control text-right sprice' data-msg-required='Selling Price is Required'/>" + "</td>" +
                    "<td><input type='number' step=any data-name='PurchasePrice' name='Addon[" + i + "].PurchasePrice' id='PurchasePrice_" + i + "' value='" + item.PurchasePrice + "'  class='PurchasePrice_" + i + " form-control text-right pprice' data-msg-required='Purchase Price is Required'/>" + "</td>" +
                    "<td><input type='number' step=any data-name='BasePrice' name='Addon[" + i + "].BasePrice' id='BasePrice_" + i + "' value='" + item.BasePrice + "'  class='BasePrice_" + i + " form-control text-right bprice' data-msg-required='Base Price is Required'/>" + "</td>" +
                    "<td><input type='number' step=any data-name='MRP' name='Addon[" + i + "].MRP' id='MRP_" + i + "' value='" + item.MRP + "'  class='MRP_" + i + " form-control text-right mprice' data-msg-required='MRP is Required'/>" + "</td></tr>";
            });
            $("#itempriceupdaters").append(ittable);
            $("#modal-itempriceupdater").modal({ show: true, backdrop: "static" });
        }
    });
}

function CurrencyConvertRate(currency) {
    $.ajax({
        url: '/Item/Convertionrate',
        type: 'POST',
        dataType: "JSON",
        data: { currency: currency },
        success: function (result) {
            $("#ConvertionRate").val(result.conv);
            var gtotal = $("#GrandTotal").val();
            var conrate = result.conv;
            var fctot = parseFloat(gtotal) * parseFloat(conrate);
            fctot = fctot.toFixed(2);
            $("#FCTotal").val(fctot);
        }
    });
}

function FCCalculation() {
    var gtotal = $("#GrandTotal").val();
    var conrate = $("#ConvertionRate").val();
    var fctot = parseFloat(gtotal) * parseFloat(conrate);
    fctot = fctot.toFixed(2);
    $("#FCTotal").val(fctot);
}


function ProjectPopup() {
    $('#modal-project').on('shown.bs.modal', function (e) {

        var date = new Date();
        date.setDate(date.getDate());
        $('.date').datepicker({
            startDate: date,
            format: 'dd-mm-yyyy',
            autoclose: true,
            allowInputToggle: true
        });
        jQuery.validator.methods["date"] = function (value, element) { return true; }

        //$(function () {
        //    $(".textareapopup").wysihtml5();
        //});

        // $("#ddlProType").select2();

        $("#ddlCust").select2({
            placeholder: 'Search Customer by Name or Code',
            minimumInputLength: 0,
            ajax: {
                url: "/Customer/SearchCustomer",
                dataType: 'json',
                delay: 50,
                data: function (params) {
                    return {
                        q: params.term,
                        page: params.page
                    };
                },
                processResults: function (data, params) {
                    params.page = params.page || 1;

                    return {
                        results: data
                    };
                },
                cache: true
            },
        });

        $("#SalesPerson").select2({
            placeholder: 'Search Sales Person by Name ',
            minimumInputLength: 0,
            ajax: {
                url: "/Employee/SearchEmployee",
                dataType: 'json',
                delay: 50,
                data: function (params) {
                    return {
                        q: params.term,
                        page: params.page,
                        x: "empty"
                    };
                },
                processResults: function (data, params) {
                    params.page = params.page || 1;

                    return {
                        results: data
                    };
                },
                cache: true
            },
        });


        $('#StartDate').datepicker({
            format: 'dd-mm-yyyy',
            autoclose: true,
            allowInputToggle: true
        });
        $('#EndDate').datepicker({
            format: 'dd-mm-yyyy',
            autoclose: true,
            allowInputToggle: true
        });
        var datetd = new Date();
        datetd.setDate(datetd.getDate());
        var datenew = new Date();
        datenew.setDate(datenew.getDate());



        $('body').on('change', '#StartDate', function (e) {
            var date = $(this).datepicker('getDate');
            if (date) {
                date.setDate(date.getDate());
            }
            // $('#ExEndDate').datepicker("setDate", date);
            $('#EndDate').datepicker("setStartDate", date);
        });



        $('body').on('change', '#ddlCust', function (e) {
            var custId = $(this).val();
            if (custId != "") {
                $.ajax({
                    url: '/Customer/GetCustomerById',
                    type: "GET",
                    dataType: "JSON",
                    data: { custId: custId },
                    success: function (result) {
                        if (result != null) {
                            $('#Location').val(result.Location);
                            $('#ContactPerson').val(result.ContactPerson);
                        }
                    }
                })
            }
        });


        $.fn.modal.Constructor.prototype.enforceFocus = function () { };
        $('#modal-create').on('shown.bs.modal', function (e) {

        });

        $('span').on('click', '.modal-create', function (e) {
            e.preventDefault();
            var url = $(this).attr('href');
            modalshow(url, '#modal-create');
            //datepickerInit();
        });
        $('#modal-create').on('submit', '#createform', function (e) {
            e.preventDefault();
            var url = $('#createform')[0].action;
            var data = $('#createform').serialize();
            createajax(url, data, '#modal-create');
        });

        $('span').on('click', '.modal-create', function (e) {
            e.preventDefault();
            var url = $(this).attr('href');
            modalshow(url, '#modal-create');
            //datepickerInit();
        });
        $('#modal-create').on('submit', '#createform', function (e) {
            e.preventDefault();
            var url = $('#createform')[0].action;
            var data = $('#createform').serialize();
            createajax(url, data, '#modal-create');
        });


        $('span').on('click', '.modal-ptype', function (e) {
            e.preventDefault();
            var url = $(this).attr('href');
            modalshow(url, '#modal-ptype');
        });

        $('#modal-ptype').on('submit', '#typeform', function (e) {
            var url = $('#modal-ptype #typeform')[0].action;

            var text = $("#TypeName").val();
            $('#ddlProType option:selected').attr("selected", null);
            $.ajax({
                type: "POST",
                url: url,
                data: $('#modal-ptype #typeform').serialize(),
                success: function (data) {
                    if (data.status) {
                        $('#modal-ptype').modal('hide');

                        var newOption = $('<option></option>');
                        newOption.val(data.Id).attr("selected", "selected");
                        newOption.html(text);
                        $('#ddlProType').append(newOption);
                    }
                    else {
                        $('.ajax_response', res_danger).text(data.message);
                        $('.AlertDiv').prepend(res_danger);
                    }
                    fadeAlert();
                }
            })

            e.preventDefault();
        });

        $('body').on('click', '.modal-close-btn', function () {
            $('#modal-ptype').modal('hide');
            $('#modal-ptype').removeData('bs.modal');
            $("button").prop('disabled', true);
        });



        $('span').on('click', '.modal-customer', function (e) {
            e.preventDefault();
            var url = $(this).attr('href');
            modalshow(url, '#modal-customer');
            //datepickerInit();
        });
        $('#modal-customer').on('submit', '#createform', function (e) {
            e.preventDefault();
            var url = $('#createform')[0].action;
            var data = $('#createform').serialize();
            createajax(url, data, '#modal-customer');
        });


        $('div').on('click', '.modal-addnew', function (e) {
            e.preventDefault();
            $(this).attr('data-target', '#modal-addnew');
            $(this).attr('data-toggle', 'modal');
        });
        $('#modal-addnew').on('submit', '#pstatform', function (e) {
            var url = $('#modal-addnew #pstatform')[0].action;

            var text = $("#StatusName").val();
            $('#ddlPStatus option:selected').attr("selected", null);
            $.ajax({
                type: "POST",
                url: url,
                data: $('#modal-addnew #pstatform').serialize(),
                success: function (data) {
                    if (data.status) {
                        $('#modal-addnew').modal('hide');

                        var newOption = $('<option></option>');
                        newOption.val(data.Id).attr("selected", "selected");
                        newOption.html(text);
                        $('#ddlPStatus').append(newOption);
                    }
                    else {
                        $('.ajax_response', res_danger).text(data.message);
                        $('.AlertDiv').prepend(res_danger);
                    }
                    fadeAlert();
                }
            })

            e.preventDefault();
        });

    });


    $('#modal-project').on('submit', '#projectform', function (e) {
        e.preventDefault();
        if ($("#projectform").valid()) {
            var url = $('#projectform')[0].action;
            var formData = new FormData(this);
            $.ajax({
                async: true,
                cache: false,
                dataType: "json",
                type: "POST",
                processData: false,
                contentType: false,
                url: url,
                data: formData,
                beforeSend: function () {
                    $("button").prop('disabled', true); // disable button
                },
                success: function (e) {
                    if (e.status) {
                        $('.ajax_response', res_success).text(e.message);
                        $('.AlertDiv').prepend(res_success);
                        $('#modal-project').modal('hide');
                        $('#modal-project').removeData('bs.modal');
                    }
                    else {
                        $('.ajax_response', res_danger).text(e.message);
                        $('.AlertDiv').prepend(res_danger);
                        // $("button").prop('disabled', false); // enable button
                    }
                    $("button").prop('disabled', false);
                }
            });
        }
    });

}

function saletypechange(type, method) {
    if (type != null) {
        var Stype = $('#SaleType').val();
        var Hire = $('#ddlHType').val();
        if (type == 1 && method == "Sale") {
            mc = 0;
            Hire = null;
        }
        else {
            var mc = $("#ddlMC").val();
        }
        var tbody = $("#normalinvoice tbody");
        if (tbody.children().length > 0) {
            tbody.children("tr").each(function () {
                var rowid = $(this).attr("id");
                var item = $("#" + rowid + " .item_name").val();
                if (item != null) {
                    $.ajax({
                        url: '/Item/GetItem',
                        type: "GET",
                        dataType: "JSON",
                        data: { itemID: item, mc: mc, Saltype: Stype, HireType: Hire },
                        success: function (result) {
                            $("#" + rowid + " .tax_percentage").val(result.Tax.toFixed(2));

                            var quantity = $("#" + rowid + " .quty").val();
                            $("#" + rowid + " .totrate").val(result.SellingPrice);
                            var rate = result.SellingPrice;
                            var subtotal = quantity * rate;

                            $("#" + rowid + " .subtotal").val(subtotal.toFixed(2));

                            var itemdiscount = $("#" + rowid + " .item_discount").val();
                            subtotal = subtotal - itemdiscount;

                            var taxAmount = subtotal * (result.Tax / 100);
                            var Total = subtotal + taxAmount;

                            $("#" + rowid + " .tot_tax").val(taxAmount.toFixed(2));
                            $("#" + rowid + " .tax").val(taxAmount.toFixed(2) + " (" + result.Tax + "%)");
                            $("#" + rowid + " .total_price").val(Total.toFixed(2));


                            CalculatetblItemListSum();
                            grandtotalcalculation();
                            paidamountcalculation();
                        }
                    });
                }
            });
        }
    }
}

$('body').on('change', '#ddlHType', function (e) {
    //var type = $(this).val();
    var method = "Hire";
    var type = $('#ddlHType').val();
    saletypechange(type, method);
});

function DateSet() {
    var ndate = $('#FromDate').val();

    $('#ToDate').datepicker({
        startDate: ndate,
        format: 'dd-mm-yyyy',
        autoclose: true,
        allowInputToggle: true
    });

}
function daysdifference(date1, date2) {
    var ONEDAY = 1000 * 60 * 60 * 24;
    var newdate1 = new Date(date1).getTime();
    var newdate2 = new Date(date2).getTime();
    var difference = Math.abs(newdate1 - newdate2);
    return Math.round(difference / ONEDAY);
}
function parseDate(input) {
    var parts = input.match(/(\d+)/g);
    return new Date(parts[0], parts[1] - 1, parts[2]);
}

function tocountweek(to, from) {
    var todate = new Date(to).getDate();
    var fromdate = new Date(from).getDate();
    var frommonth = new Date(from).getMonth();
    var fromyear = new Date(from).getYear();
    var tomonth = new Date(to).getMonth();
    var toyear = new Date(to).getYear();
    var daysdiff = daysdifference(from, to);
    var wholeweek = Math.floor(daysdiff / 7);
    var quotient = daysdiff % 7;
    var totalweek = (quotient > 0) ? (wholeweek + 1) : wholeweek;
    return totalweek;
}

function tocountmonth(to, from) {
    var fromyear = new Date(from).getYear();
    var toyear = new Date(to).getYear();
    var todate = new Date(to).getDate();
    var fromdate = new Date(from).getDate();
    var frommonth = new Date(from).getMonth();
    var tomonth = new Date(to).getMonth();
    var month;
    if (fromyear != toyear) {
        var fromtotmonth = ((fromyear - 1) * 12) + frommonth;
        var tototmonth = ((toyear - 1) * 12) + tomonth;
        var Netdiff = tototmonth - fromtotmonth;
        month = (todate >= fromdate) ? (Netdiff + 1) : Netdiff;
    }
    else {
        month = (todate >= fromdate) ? (tomonth - frommonth) + 1 : (tomonth - frommonth);
    }
    return month;
}

function tocountyear(to, from) {
    var fromyear = new Date(from).getYear();
    var toyear = new Date(to).getYear();
    var year = (fromyear != toyear) ? year(fromyear, toyear) : 1;
    return year;
}

function year(fromyear, toyear) {
    var todate = new Date(to).getDate();
    var fromdate = new Date(from).getDate();
    var frommonth = new Date(from).getMonth();
    var tomonth = new Date(to).getMonth();

    var fromtotmonth = ((fromyear - 1) * 12) + frommonth;
    var tototmonth = ((toyear - 1) * 12) + tomonth;
    var Netmnth = tototmonth - fromtotmonth;
    var wholeyear = Math.floor(Netmnth / 12);
    var quotient = Netmnth % 12;
    var year = (quotient > 0) ? (wholeyear + 1) : wholeyear;
    return year;
}

function stypechange(type) {
    if (type != null) {
        if (type == 2) {
            var TaxP = 0;
            var tbody = $("#normalinvoice tbody");
            if (tbody.children().length > 0) {
                tbody.children("tr").each(function () {
                    var rowid = $(this).attr("id");
                    var item = $("#" + rowid + " .item_name").val();
                    var subtot = $("#" + rowid + " .subtotal").val();
                    if (item != null) {
                        $("#" + rowid + " .tax_percentage").val(0);
                        $("#" + rowid + " .tot_tax").val(0);
                        $("#" + rowid + " .tax").val("0.00" + " (" + 0 + "%)");
                        $("#" + rowid + " .total_price").val(subtot);
                    }
                });
                CalculatetblItemListSum();
                grandtotalcalculation();
                paidamountcalculation();
            }
        } else {

            var tbody = $("#normalinvoice tbody");
            if (tbody.children().length > 0) {
                tbody.children("tr").each(function () {
                    var rowid = $(this).attr("id");
                    var item = $("#" + rowid + " .item_name").val();
                    if (item != null) {
                        $.ajax({
                            url: '/Item/GetItem',
                            type: "GET",
                            dataType: "JSON",
                            data: { itemID: item },
                            success: function (result) {
                                $("#" + rowid + " .tax_percentage").val(result.Tax.toFixed(2));

                                var quantity = $("#" + rowid + " .quty").val();
                                var rate = $("#" + rowid + " .totrate").val();
                                var subtotal = quantity * rate;
                                $("#" + rowid + " .subtotal").val(subtotal.toFixed(2));

                                var itemdiscount = $("#" + rowid + " .item_discount").val();
                                subtotal = subtotal - itemdiscount;

                                var taxAmount = subtotal * (result.Tax / 100);
                                var Total = subtotal + taxAmount;

                                $("#" + rowid + " .tot_tax").val(taxAmount.toFixed(2));
                                $("#" + rowid + " .tax").val(taxAmount.toFixed(2) + " (" + result.Tax + "%)");
                                $("#" + rowid + " .total_price").val(Total.toFixed(2));


                                CalculatetblItemListSum();
                                grandtotalcalculation();
                                paidamountcalculation();
                            }
                        });
                    }


                });


            }
        }


    }
}



function ptypechange(type) {
    if (type != null) {
        if (type == 2) {
            var TaxP = 0;
            var tbody = $("#normalinvoice tbody");
            if (tbody.children().length > 0) {
                tbody.children("tr").each(function () {
                    var rowid = $(this).attr("id");
                    var item = $("#" + rowid + " .item_name").val();
                    var subtot = $("#" + rowid + " .subtotal").val();
                    if (item != null) {
                        $("#" + rowid + " .tax_percentage").val(0);
                        $("#" + rowid + " .tot_tax").val(0);
                        $("#" + rowid + " .tax").val("0.00" + " (" + 0 + "%)");
                        $("#" + rowid + " .total_price").val(subtot);
                    }
                });
                CalculatetblItemListSum();
                grandtotalcalculation();
                paidamountcalculation();

            }
        } else {

            var tbody = $("#normalinvoice tbody");
            if (tbody.children().length > 0) {
                tbody.children("tr").each(function () {
                    var rowid = $(this).attr("id");
                    var item = $("#" + rowid + " .item_name").val();
                    if (item != null) {
                        $.ajax({
                            url: '/Item/GetItem',
                            type: "GET",
                            dataType: "JSON",
                            data: { itemID: item },
                            success: function (result) {
                                $("#" + rowid + " .tax_percentage").val(result.Tax.toFixed(2));

                                var quantity = $("#" + rowid + " .quty").val();
                                var rate = $("#" + rowid + " .totrate").val();
                                var subtotal = quantity * rate;
                                $("#" + rowid + " .subtotal").val(subtotal.toFixed(2));

                                var itemdiscount = $("#" + rowid + " .item_discount").val();
                                subtotal = subtotal - itemdiscount;

                                var taxAmount = subtotal * (result.Tax / 100);
                                var Total = subtotal + taxAmount;

                                $("#" + rowid + " .tot_tax").val(taxAmount.toFixed(2));
                                $("#" + rowid + " .tax").val(taxAmount.toFixed(2) + " (" + result.Tax + "%)");
                                $("#" + rowid + " .total_price").val(Total.toFixed(2));


                                CalculatetblItemListSum();
                                grandtotalcalculation();
                                paidamountcalculation();

                            }
                        });
                    }


                });


            }
        }


    }
}



// Update Discount and rowSubTotal in each items based on discount %
function Discounts() {
    var DisPercent = $("#DisPercent").val();
    var tbody = $("#normalinvoice tbody")
    var DisPrice = (parseFloat(DisPercent) / 100).toFixed(2);

    if (tbody.children().length > 0 && !isNaN(DisPrice)) {
        tbody.children("tr").each(function () {
            var rowid = $(this).attr("id");
            var item = $("#" + rowid + " .item_name").val();
            if ((item != null) && (DisPercent != null)) {
                if ((DisPercent > 0) && (DisPrice > 0)) {
                    var subtot = $("#" + rowid + " .subtotal").val();
                    var itemdiscount = (parseFloat(subtot) * parseFloat(DisPrice)).toFixed(2);
                    $("#" + rowid + " .item_discount").val(itemdiscount);
                }
            }
            var dataid = $("#" + rowid + " .item_name").attr("data-id");
            rowSubTotal(dataid);
            CalculatetblItemListSum();
            grandtotalcalculation();
            paidamountcalculation();

        });
    }
}



// update Discount % based upon discount
function DiscountChng(arg) {
    var argval = 1;
    if (arg != 1) {
        var itemsubtot = $(".sub_total_" + argval).val();
        var itemDiscount = $(".item_discount" + argval).val();
        var itemDiscPer = (parseFloat(itemDiscount) / parseFloat(itemsubtot)) * 100;
        var tbody = $("#normalinvoice tbody");
        if (tbody.children().length > 0) {
            tbody.children("tr").each(function () {
                var rowid = $(this).attr("id");
                var item = $("#" + rowid + " .item_name").val();
                if (item != null) {
                    var subtot = $("#" + rowid + " .subtotal").val();
                    var Discount = $("#" + rowid + " .item_discount").val();
                    var DiscPercent = (parseFloat(Discount) / parseFloat(subtot)) * 100;
                    if (DiscPercent != itemDiscPer) {
                        $("#DisPercent").val("");

                    }
                }
            });
        }
    }
}
function VatVoucherWisePurchaseOrder() {
    var stype = $("#PurchaseType").val();
    if (stype == 3) {
        $(".vwise").show();
        $(".item_discount").val(0);
        $(".item_discount").prop('disabled', true);
    } else {
        $(".item_discount").prop('disabled', false);
        if (stype == 1) {
            $(".vwise").show();
        } else {
            $(".vwise").hide();
        }
    }
}
function bspcalculate() {
    var gtTotal = parseFloat($("#total").text());
    var taxAmt = parseFloat($("#total_tax_amount").text());

    var subtot = 0;
    var stype = $("#PurchaseType").val();//voucher wise tax
    if (stype == 3) {
        subtot = parseFloat(gtTotal) - parseFloat(taxAmt);
    } else {
        subtot = parseFloat(gtTotal);
    }

    $("#addbillsundry tr").each(function () {
        var bsval = $(this).find('.bsvalue').val();
        var amttype = $(this).find('.bsamttype').val();
        var type = $(this).find('.bstype').val();

        if (amttype == "%") {
            if (type == 0) {
                var perval = (subtot) * (bsval / 100);
                $(this).find('.bsamt').val(parseFloat(perval).toFixed(2));
                subtot = parseFloat(subtot + perval).toFixed(2);
            } else {
                var perval = (subtot) * (bsval / 100);
                $(this).find('.bsamt').val(parseFloat(perval).toFixed(2));
                subtot = parseFloat(subtot - perval).toFixed(2);
            }
        }
    });
}