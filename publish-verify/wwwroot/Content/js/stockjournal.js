var count = 1, type = '';
limits = 500;
//Add generateditem Row ItemSubTotal GeneratedTotal action
function addrow(t, ItemUnit, ItemTotalAmount, ItemQuantity, Item, ItemCode, ItemName, ItemUnitPrice, ItemWithCode, itemdata) {
    if (count == limits) alert("You have reached the limit of adding " + count + " inputs");
    else {
        var Option = "";
        var optionunit = "";
        var required = "";
        var slno = $('#Generated tr').length + 1;
        var a = "item_name" + count, tabindex = count * 5;
        var row = "<tr class='item_' id='item_" + count + "'>";
        var data = "";
        var price = 0;
        var baseprice = 0;
        var mrp = 0;
        var htdata = "";

        tab1 = tabindex + 1;
        tab2 = tabindex + 2;
        tab3 = tabindex + 3;

        if (Item != null) {
            row = "<tr class='item_" + Item + "' id='item_" + count + "'>";
            Option = "<option value='" + Item + "'>" + ItemWithCode + "</option>";
        }
        if (count == 1) {
            required = 'required="required"';
        }
        //if (action != '') {
        //    type = action;
        //}

        var inote = "";
        if (itemdata) {
            inote = itemdata.note;
        }

        if (itemdata) {
            htdata = "<div class='minstock_" + count + "'";

            price = itemdata.SellingPrice;
            baseprice = itemdata.BasePrice;
            mrp = itemdata.MRP;

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
        var itemaddbtn = "<span class='input-group-btn'><a type='button' href='/Item/AddItem' class='modal-create-lg btn btn-success btn-flat'><i class='fa fa-1x fa-plus-circle'></i></a></span>";

        data = "<td class='text-center'> " + slno + " </td>" +
                "<td class='input-group input-group-sm'><select data-name='Item' class='form-control item_name' " + required + " data-id='" + count + "' placeholder='Item Name' id='item_name_" + count + "'  data-val-required='The Item field is required' onchange='GetItemdetails(this," + count + ",\"" + type + "\")'>" + Option + "</select> " + itemaddbtn +
                "<td style='width:100px;'><select class='form-control units unit_name_" + count + "' id='unit_name_" + count + "' data-id='" + count + "' id='unit_name' onchange='unitchange(this," + count + ",\"" + type + "\");'></select>" + " </td> " +
                "<td><input type='number' name='product_qnt[]' " + required + " onchange='quantity_change(" + count + ",\"" + type + "\");' id='item_quantity_" + count + "' value='" + ItemQuantity + "' readonly='readonly' class='item_quantity_" + count + " form-control text-right quantity' placeholder='0.00' min='0' tabindex='" + tab2 + "'/></td> " +
                "<td><input type='number' name='product_rate[]' " + required + " onchange='rate_change(" + count + ",\"" + type + "\");' id='item_price_" + count + "' value='" + ItemUnitPrice + "' readonly='readonly' class='price_item_" + count + " form-control text-right sell_price' placeholder='0.00' min='0' step='any' /></td> " + "<input type='hidden' data-value='" + price + "' value='" + baseprice + "' name='base_rate' id='base_rate_" + count + "'></td>" +
                "<td><input class='total_price total_price_" + count + " form-control text-right total_price_' type='text' name='total_price[]' value='" + ItemTotalAmount + "' id='total_price_" + count + "' value='0.00' readonly='readonly'/> </td> " +
                "<td class='text-center'><button tabindex='" + tab3 + "' style='text-align: right;' class='btn btn-danger' type='button' value='Delete' onclick='deleteRow(this)'><i class='fa fa-trash fa-1x'></i></button>" + htdata + " </td>";
        row += data + "</tr>";
        $('#' + t).append(row);
        // $('#item_ .item_name').focus();
        searchItem();
        if (itemdata) {
            quantity_change(count, type, 'foredit');
            createUnitList(itemdata, count);
        }
        else
            quantity_change(count, type);
        count++;
        setTabIndex();
    }
}

//Add consumeditem Row Caction,
var Ccount = 1, type = '';
limits = 500;
function ItemConsumed(Ct, CItemUnit, CItemTotalAmount, CItemQuantity, CItem, CItemCode, CItemName, CItemUnitPrice, CItemWithCode, Citemdata, Caction) {
    if (Ccount == limits) alert("You have reached the limit of adding " + Ccount + " inputs");
    else {
        var Option = "";
        var Coptionunit = "";
        var required = "";
        var Cslno = $('#Consumed tr').length + 1;
        var Ca = "Citem_name" + Ccount, tabindex = Ccount * 5;
        var Crow = "<tr class='Citem_' id='Citem_" + Ccount + "'>";
        var Cdata = "";
        var Cprice = 0;
        var Cbaseprice = 0;
        var Cmrp = 0;
        var Chtdata = "";

        tab1 = tabindex + 1;
        tab2 = tabindex + 2;
        tab3 = tabindex + 3;

        if (CItem != null) {
            Crow = "<tr class='Citem_" + CItem + "' id='Citem_" + Ccount + "'>";
            Option = "<option value='" + CItem + "'>" + CItemWithCode + "</option>";
        }
        if (Ccount == 1) {
            required = 'required="required"';
        }

        if (Caction != '') {
            type = Caction;
        }

        var Cinote = "";
        if (Citemdata) {
            Cinote = Citemdata.note;
        }
        if (Citemdata) {
            Cprice = Citemdata.SellingPrice;
            // Cbaseprice = Citemdata.CBasePrice;
            Cmrp = Citemdata.CMRP;
            Chtdata = "<div class='Cminstock_" + Ccount + "'";
            if (Citemdata.KeepStock == true) {
                var Cqntmin = 0;
                if (Citemdata.CItemUnit == Citemdata.CItemUnitID) {
                    Cqntmin = CItemQuantity * Citemdata.ConFactor;
                }
                if (Citemdata.CItemUnit == Citemdata.SubUnitId) {
                    Cqntmin = CItemQuantity;
                }
                Ctotalstock = Citemdata.total + Cqntmin;
                Cminstock = Citemdata.CMinStock * Citemdata.ConFactor;
                Chtdata += " data-keeps='yes' data-min='" + Cminstock + "' data-confactor='" + Citemdata.ConFactor + "' data-stock='" + Ctotalstock + "'>";
            }
            else {
                Chtdata += " data-keeps='no' >";
            }
            if ($(".Cminstock_" + Ccount).length) {
                $(".Cminstock_" + Ccount).remove();
            }
        }
        var Citemaddbtn = "<span class='input-group-btn'><a type='button' href='/Item/AddItem' class='modal-create-lg btn btn-success btn-flat'><i class='fa fa-1x fa-plus-circle'></i></a></span>";

        Cdata = "<td class='text-center'> " + Cslno + " </td>" +
                "<td class='input-group input-group-sm'><select data-name='Item' class='form-control Citem_name' data-id='" + Ccount + "' placeholder='Item Name' id='Citem_name_" + Ccount + "'  data-val-required='The Item field is required' onchange='CGetItemdetails(this," + Ccount + ",\"" + type + "\")'>" + Option + "</select> " + Citemaddbtn + "<input type='hidden' data-value='" + Cprice + "' value='" + Cbaseprice + "' name='C_base_rate' id='C_base_rate_" + count + "'></td>" +
                "<td style='width:100px;'><select class='form-control units C_unit_name_" + Ccount + "' id='C_unit_name_" + Ccount + "' " + " data-id='" + Ccount + "' id='C_unit_name_' onchange='C_unitchange(this," + Ccount + ",\"" + type + "\");'></select>" + "</td> " +
                "<td><input data-name='C_quantity' type='number' name='product_qty[]' onchange='C_quantity_change(" + Ccount + ",\"" + type + "\")' id='C_item_quantity_" + Ccount + "' value='" + CItemQuantity + "' readonly='readonly' class='C_item_quantity_" + Ccount + " form-control text-right C_quantity' placeholder='0.00' min='0' tabindex='" + tab2 + "'/></td> " +
                "<td><input data-name='C_price' type='number' name='C_product_rate[]' onchange='C_rate_change(" + Ccount + ",\"" + type + "\")' id='C_item_price_" + Ccount + "' value='" + CItemUnitPrice + "' readonly='readonly' class='C_price_item_" + Ccount + " form-control text-right sell_price' placeholder='0.00' min='0' step='any' /> </td> " +
                "<td><input class='C_total_price C_total_price_" + Ccount + " form-control text-right C_total_price_' type='text' name='C_total_price[]' value='" + CItemTotalAmount + "' id='C_total_price_" + Ccount + "' value='0.00' readonly='readonly'/> </td> " +
                "<td class='text-center'><button tabindex='" + tab3 + "' style='text-align: right;' class='btn btn-danger' type='button' value='Delete' onclick='CdeleteRow(this)'><i class='fa fa-trash fa-1x'></i></button>" + Chtdata + " </td>";
        Crow += Cdata + "</tr>";
        $('#' + Ct).append(Crow);
        // $('#item_ .item_name').focus();
        CsearchItem();
        if (Citemdata) {
            C_quantity_change(Ccount, type, 'foredit');
            C_createUnitList(Citemdata, Ccount);
        }
        else
            C_quantity_change(Ccount, type);
        Ccount++;
        CsetTabIndex();
    }
}

//item details
function GetItemdetails(selectObject, dataid, action) {
    $("#item_price_" + dataid).attr('readonly', false);
    $("#item_quantity_" + dataid).attr('readonly', false);
    if (selectObject.value) {
        var ItemId = selectObject.value;
        if (ItemId != null) {
            if ($(".item_" + ItemId).length > 0) {
                if ($(".item_" + ItemId).length < 2) {
                    if (confirm('Are you sure want to Add this item Again?')) {
                        itemUpdate(selectObject, dataid, action);
                    }
                    else {
                        $(selectObject).val(null).trigger('change');
                    }
                }
                else {
                    alert("You Cannot Add same Item More than 2 Times");
                    $(selectObject).val(null).trigger('change');
                }
            }
            else {
                itemUpdate(selectObject, dataid, action);
            }
        }
    }
}
//item details
function CGetItemdetails(selectObject, dataid, Caction) {
    $("#item_price_" + dataid).attr('readonly', false);
    $("#item_quantity_" + dataid).attr('readonly', false);
    if (selectObject.value) {
        var ItemId = selectObject.value;
        if (ItemId != null) {
            if ($(".Citem_" + ItemId).length > 1) {
                if ($(".Citem_" + ItemId).length == 2) {
                    alert("Cannot Add an Item More than 2 times !!");
                    $("#Citem_name_" + dataid).val(null).trigger("change");
                }
            } else {
                if ($(".Citem_" + ItemId).length > 0) {
                    if (confirm('Are you sure want to Add this item Again?')) {
                        CitemUpdate(selectObject, dataid, Caction);
                    }
                    else {
                        $("#Citem_name_" + dataid).val(null).trigger("change");
                    }
                }
                else {
                    CitemUpdate(selectObject, dataid, Caction);
                    CCalculatetblItemListSum();
                }
            }
        }
    }
}
function CitemUpdate(selectObject, dataid, action) {
    $.ajax({
        url: '/Item/GetItem',
        type: "GET",
        dataType: "JSON",
        data: { itemID: selectObject.value },
        success: function (result) {
            C_createUnitList(result, dataid);
            $(".C_price_item_" + dataid).val(result.SellingPrice);
            $("#C_item_quantity_" + dataid).val(1);
            C_rowSubTotal(dataid);
            CCalculatetblItemListSum();
            $(selectObject).closest('tr').attr('class', "Citem_" + result.ItemID);
            if ($(".Citem_").length == 0) {
                ItemConsumed('Consumed', '', '', '0.00');
            }
            $('.C_unit_name_' + dataid).focus();

        }
    });
}
function CCalculatetblItemListSum() {
    var qty = $(".C_quantity").val();
    var tbody = $("#consumeditem tbody");
    if (tbody.children().length > 0) {
        var gtTotal = 0;
        var gtQty = 0;
        var gtSubTotal = 0;
        $(".C_total_price").each(function () {
            var indtot = $(this).val();
            gtTotal = parseFloat(gtTotal) + parseFloat(indtot);
        });
        $(".C_quantity").each(function () {
            var subQty = this.value;
            gtQty = parseFloat(gtQty) + parseFloat(subQty);
        });
        $("[id$=ConsumedtotalAmt]").text((gtTotal).toFixed(2));
        $("[id$=ConsumedtotalQty]").text((gtQty).toFixed(2));
    }

}
// update item details
function itemUpdate(selectObject, dataid, action) {
    $.ajax({
        url: '/Item/GetItem',
        type: "GET",
        dataType: "JSON",
        data: { itemID: selectObject.value },
        success: function (result) {
            minstockupdate(result, dataid);
            if (result.KeepStock == true) {
                createUnitList(result, dataid);
                $(".price_item_" + dataid).val(result.SellingPrice);
                $("#item_quantity_" + dataid).val(1);
                $("#base_rate_" + dataid).attr("data-value", result.SellingPrice);
                rowSubTotal(dataid);
                CalculatetblItemListSum();
                $(selectObject).closest('tr').attr('class', "item_" + result.ItemID);
                if ($(".item_").length == 0) {
                    addrow('Generated', '', '', '0.00');
                }
                $('.unit_name_' + dataid).focus();
                CalculatetblItemListSum();
            }
            else {
                alert("This Item is Out of Stock!!!");
                var classname = $($("#item_quantity_" + dataid)).closest('tr').attr('class');
                if (classname != 'item_') {
                    $("." + classname + " .btn-danger").click();
                }
                else {
                    $("#item_name_" + dataid).val(null).trigger("change");
                }
            }

        }
    });
}
function CalculatetblItemListSum() {
    var qty = $(".quantity").val();
    var tbody = $("#generateditem tbody");
    if (tbody.children().length > 0) {
        var gtTotal = 0;
        var gtQty = 0;
        var gtSubTotal = 0;
        $(".total_price").each(function () {
            var indtot = $(this).val();
            gtTotal = parseFloat(gtTotal) + parseFloat(indtot);
        });
        $(".quantity").each(function () {
            var subQty = $(this).val();
            subQty = subQty || 0;
            gtQty = parseFloat(gtQty) + parseFloat(subQty);
        });
        var len = tbody.children().length - 1;
        $("[id$=ItemCount]").text((len));
        $("[id$=GeneratedtotalAmt]").text((gtTotal).toFixed(2));
        $("[id$=GeneratedtotalQty]").text((gtQty).toFixed(2));
    }
}

function minstockcheck(arg) {
    var keepstock = $(".minstock_" + arg).attr('data-keeps');//check keepstock     
    if (keepstock == "yes") {
        var index = $('#unit_name_' + arg).prop('selectedIndex');
        var unitname = $('#unit_name_' + arg).find('option:selected').text();
        var minstock = parseFloat($(".minstock_" + arg).attr('data-min'));
        var confactor = parseFloat($(".minstock_" + arg).attr('data-confactor'));
        var stock = parseFloat($(".minstock_" + arg).attr('data-stock'));
        var quantity = parseFloat($(".item_quantity_" + arg).val());
        var qty = 0;
        var classn = $("#item_" + arg).attr('class');
        $("." + classn).each(function () {
            var rowid = $(this).attr('id');
            var arr = rowid.split('_');
            var arg1 = arr[1];
            var index1 = $("#" + rowid + " .units").prop('selectedIndex');
            var curent = $("#" + rowid + " .quantity").val();
            var confactor1 = parseFloat($("#" + rowid + "  .minstock_" + arg1).attr('data-confactor'));
            if (index == 0) {
                qty += (curent * confactor1);
            }
            else {
                qty += curent;
            }
        });
        if (index == 0) {
            stock = stock - (qty - quantity);
            minstock = minstock / confactor;
            stock = stock / confactor;
            var tostock = stock - quantity;
            var totstock = tostock / confactor;
            if (totstock <= minstock && totstock >= 0) {
                alert("Stock Exceeds Minimum Stock");
            }
            if (totstock < 0) {
                stock = stock.toFixed(2);
                alert("This Item Is Going To Out of Stock!!! Only " + stock + " " + unitname + "Items Are Available In Stock..");
                $(".item_quantity_" + arg).val(parseInt(stock));
            }
        } else {
            stock = stock - (qty - quantity);
            var totstock = stock - quantity;
            if (totstock <= minstock && totstock >= 0) {
                alert("Stock Exceeds Minimum Stock");
            }
            if (totstock < 0) {
                alert("This Item Is Going To Out of Stock!!! Only " + stock + " " + unitname + " Items Are Available In Stock..");
                $(".item_quantity_" + arg).val(stock);
            }
        }
    }
}

//check minimum stock
function minstockupdate(result, dataid) {
    var htdata = "<div class='minstock_" + dataid + "'";
    if (result.KeepStock == true) {
        totalstock = result.total;
        minstock = result.MinStock * result.ConFactor;
        //if ($(".item_" + result.ItemID).length > 1) {
        //    $(".item_" + result.ItemID).each(function () {
        //        var rowid = $(this).attr('id');
        //        var arr = rowid.split('_');
        //        var arg = arr[1];
        //        alert(arg);
        //        if (arg != dataid) {
        //            var index = $("#" + rowid + " .unitselection").prop('selectedIndex');
        //            var unitname = $("#" + rowid + " .unitselection").find('option:selected').text();
        //            var confactor = parseFloat($("#" + rowid + "  .minstock_" + arg).attr('data-confactor'));
        //            var quanty = parseFloat($("#" + rowid + "  .minstock_" + arg).attr('data-confactor'));
        //            if (index == 0) {
        //                totalstock -= (quanty * confactor);
        //            }
        //            else {
        //                totalstock -= quanty;
        //            }
        //        }
        //    });
        //}
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


function C_minstockcheck(arg) {
    var keepstock = $(".Cminstock_" + arg).attr('data-keeps');
    if (keepstock == "yes") {
        var index = $('#C_unit_name_' + arg).prop('selectedIndex');
        var unitname = $('#C_unit_name_' + arg).find('option:selected').text();
        var Cminstock = parseFloat($(".Cminstock_" + arg).attr('data-min'));
        var confactor = parseFloat($(".Cminstock_" + arg).attr('data-confactor'));
        var stock = parseFloat($(".Cminstock_" + arg).attr('data-stock'));
        var quantity = parseFloat($(".C_item_quantity_" + arg).val());
        var qty = 0;
        var classn = $("#Citem_" + arg).attr('class');
        $("." + classn).each(function () {
            var rowid = $(this).attr('id');
            var arr = rowid.split('_');
            var arg1 = arr[1];
            var index1 = $("#" + rowid + " .C_units").prop('selectedIndex');
            var curent = $("#" + rowid + " .C_quantity").val();
            var confactor1 = parseFloat($("#" + rowid + "  .Cminstock_" + arg1).attr('data-confactor'));
            if (index == 0) {
                qty += (curent * confactor1);
            }
            else {
                qty += curent;
            }
        });
        if (index == 0) {
            stock = stock - (qty - quantity);
            Cminstock = Cminstock / confactor;
            stock = stock / confactor;
            var tostock = stock - quantity;
            var totstock = tostock / confactor;
            if (totstock <= Cminstock && totstock >= 0) {
                alert("Stock Exceeds Minimum Stock");
            }
            if (totstock < 0) {
                stock = stock.toFixed(2);
                alert("This Item Is Going To Out of Stock!!! Only " + stock + " " + unitname + "Items Are Available In Stock..");
                $(".C_item_quantity_" + arg).val(parseInt(stock));
            }
        } else {
            stock = stock - (qty - quantity);
            var totstock = stock - quantity;
            if (totstock <= Cminstock && totstock >= 0) {
                alert("Stock Exceeds Minimum Stock");
            }
            if (totstock < 0) {
                alert("This Item Is Going To Out of Stock!!! Only " + stock + " " + unitname + " Items Are Available In Stock..");
                $(".C_item_quantity_" + arg).val(stock);
            }
        }
    }
}
//check minimum stock
//function minstockupdate(result, dataid) {
//    var htdata = "<div class='Cminstock_" + dataid + "'";
//    if (result.KeepStock == true) {
//        totalstock = result.total;
//        minstock = result.MinStock * result.ConFactor;
//        //if ($(".item_" + result.ItemID).length > 1) {
//        //    $(".item_" + result.ItemID).each(function () {
//        //        var rowid = $(this).attr('id');
//        //        var arr = rowid.split('_');
//        //        var arg = arr[1];
//        //        alert(arg);
//        //        if (arg != dataid) {
//        //            var index = $("#" + rowid + " .unitselection").prop('selectedIndex');
//        //            var unitname = $("#" + rowid + " .unitselection").find('option:selected').text();
//        //            var confactor = parseFloat($("#" + rowid + "  .minstock_" + arg).attr('data-confactor'));
//        //            var quanty = parseFloat($("#" + rowid + "  .minstock_" + arg).attr('data-confactor'));
//        //            if (index == 0) {
//        //                totalstock -= (quanty * confactor);
//        //            }
//        //            else {
//        //                totalstock -= quanty;
//        //            }
//        //        }
//        //    });
//        //}
//        htdata += " data-keeps='yes' data-min='" + minstock + "' data-confactor='" + result.ConFactor + "' data-stock='" + totalstock + "'>";
//    }
//    else {
//        htdata += " data-keeps='no' >";
//    }
//    if ($(".Cminstock_" + dataid).length) {
//        $(".Cminstock_" + dataid).remove();
//    }
//    $('#Citem_' + dataid).append(htdata);
//}

function rowSubTotal(arg) {
    var quantity = $(".item_quantity_" + arg).val();
    var rate = $(".price_item_" + arg).val();
    var subtotal = parseFloat(quantity) * parseFloat(rate);
    subtotal = subtotal || 0;
    $(".total_price_" + arg).val(parseFloat(subtotal.toFixed(2)));
}
function C_rowSubTotal(arg) {
    var quantity = $(".C_item_quantity_" + arg).val();
    var rate = $(".C_price_item_" + arg).val();
    var subtotal = parseFloat(quantity) * parseFloat(rate);
    subtotal = subtotal || 0;
    $(".C_total_price_" + arg).val(subtotal.toFixed(2));
}
// search item
function searchItem() {
    var selecteditem = new Array();
    $(".item_name").each(function () {
        selecteditem.push($(this).val());
    });

    $(".item_name").select2({
        placeholder: 'Search Item by Name or Code',
        minimumInputLength: 0,
        ajax: {
            url: "/Item/SearchItem",
            dataType: 'json',
            type: "POST",
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    ItemID: selecteditem,
                    page: params.page || 0
                };
            },
            processResults: function (data, params) {
                params.page = params.page || 0;
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
    });
    rate_change(count);
}
function CsearchItem() {
    var selecteditem = new Array();
    $(".Citem_name").each(function () {
        selecteditem.push($(this).val());
    });

    $(".Citem_name").select2({
        placeholder: 'Search Item by Name or Code',
        minimumInputLength: 0,
        ajax: {
            url: "/Item/SearchItem",
            dataType: 'json',
            type: "POST",
            delay: 50,
            Cdata: function (params) {
                return {
                    q: params.term || "",
                    ItemID: selecteditem,
                    page: params.page || 0
                };
            },
            processResults: function (Cdata, params) {
                params.page = params.page || 0;
                return {
                    results: Cdata,
                    pagination: {
                        //more: (params.page * 10) < 50
                        more: true
                    }
                };
            },
            cache: true
        },
    });
    C_rate_change(Ccount);
}

function quantity_change(arg, type, foredit) {
    //if ($("#item_name_" + arg).val() != null) {
        minstockcheck(arg);
        rowSubTotal(arg);
        CalculatetblItemListSum();
    //}
    //else {
    //    $(".item_quantity_" + arg).val(0);

    //}
}

function unitchange(selectObject, arg, action) {
    minstockcheck(arg);
    var index = $('#unit_name_' + arg).prop('selectedIndex');

    if (index == 1) {
        var unitId = parseFloat($('#unit_name_' + arg).val());
        var cfactor = parseFloat($('#cfactor_' + arg).val());
        var price = parseFloat($("#base_rate_" + arg).attr("data-value"));
        var newprice = parseFloat(price / cfactor);
        $(".price_item_" + arg).val(newprice.toFixed(2));
    } else {
        var unitId = parseFloat($('#unit_name_' + arg).val());
        var price = parseFloat($("#base_rate_" + arg).attr("data-value"));
        $(".price_item_" + arg).val(price.toFixed(2));
    }
    rowSubTotal(arg);
    CalculatetblItemListSum();
}
function C_unitchange(selectObject, arg, action) {
    C_minstockcheck(arg);
    var index = $('#C_unit_name_' + arg).prop('selectedIndex');
    if (index == 1) {
        var unitId = parseFloat($('#C_unit_name_' + arg).val());
        var cfactor = parseFloat($('#cfactor_' + arg).val());
        var price = parseFloat($("#C_base_rate_" + arg).attr("data-value"));
        var newprice = parseFloat(price / cfactor);
        $(".C_price_item_" + arg).val(newprice.toFixed(2));
    } else {
        var unitId = parseFloat($('#C_unit_name_' + arg).val());
        var price = parseFloat($("#C_base_rate_" + arg).attr("data-value"));
        $(".C_price_item_" + arg).val(price.toFixed(2));
    }

    C_rowSubTotal(arg);
    CCalculatetblItemListSum();
}
function rate_change(arg, type, foredit) {
    //if ($("#item_name_" + arg).val() != null) {
        minstockcheck(arg);
        var baserate = $("#base_rate_" + arg).val();
        var rate = $(".price_item_" + arg).val();

        if (parseFloat(baserate) > parseFloat(rate) && type == 'sales' && parseFloat(rate) > 0 && foredit != 'foredit') {
            alert("Selling price is less than Base Price ");
        }
        rowSubTotal(arg);
        CalculatetblItemListSum();
    //}
    //else {
    //    $('#item_price_' + arg).val(0);
    //}

}

function C_rate_change(arg, type, foredit) {
    //if ($("#Citem_name_" + arg).val() != null) {
        minstockcheck(arg);
        var baserate = $("#C_base_rate_" + arg).val();
        var rate = $(".C_price_item_" + arg).val();

        if (parseFloat(baserate) > parseFloat(rate) && type == 'sales' && parseFloat(rate) > 0 && foredit != 'foredit') {
            alert("Selling price is less than Base Price ");
        }
        C_rowSubTotal(arg);
        CCalculatetblItemListSum();
    //}
    //else {
    //    $(".C_price_item_" + arg).val(0);
    //}
}
function C_quantity_change(arg, type, foredit) {
    //if ($("#Citem_name_" + arg).val() != null) {
        C_minstockcheck(arg);
        C_rowSubTotal(arg);
        CCalculatetblItemListSum();
    //}
    //else {
    //    $(".C_item_quantity_" + arg).val(0);
    //}
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
function CsetTabIndex() {
    var j = 1;
    $('body').find('input,textarea,select,button, .select2-container .select2-selection__rendered').not(".select2-hidden-accessible").not(":hidden").each(function (i) {
        if (!$(this).hasClass("select2-hidden-accessible") && !$(this).is(":hidden")) {
            $(this).attr('tabindex', j);
            j++;
        }
        if ($(this).closest("tr").hasClass("Citem_") && !$(this).hasClass("select2-selection__rendered")) {
            $(this).attr('tabindex', -1);
        }
    });
}

//Delete a row of table
function deleteRow(t) {
    var classname = $(t).closest('tr').attr('class');
    if (classname == 'item_') alert("Sorry You Can't Delete This Row.");
    else {
        var r = confirm("Are you sure you want to delete this..?");
        if (r == true) {
            var e = t.parentNode.parentNode;
            e.parentNode.removeChild(e);
        }
    }
    var i = 1;
    $('#addrow tr').each(function () {
        $(this).find('td:first').text(i);
        i++;
    });
    CalculatetblItemListSum();
}
function CdeleteRow(Ct) {
    var classname = $(Ct).closest('tr').attr('class');

    if (classname == 'Citem_') alert("Sorry You Can't Delete This Row.");
    else {
        var e = Ct.parentNode.parentNode;
        e.parentNode.removeChild(e);
    }
    var i = 1;
    $('#ItemConsumed tr').each(function () {
        $(this).find('td:first').text(i);
        i++;
    });
    CCalculatetblItemListSum();
}

// create units based on primary and secondary
function createUnitList(result, dataid) {
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

function C_createUnitList(result, dataid) {
    $('#C_unit_name_' + dataid).empty();
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

            $('#C_unit_name_' + dataid).append(newOption);
            $('#C_unit_name_' + dataid).append(newOption1);
        }
        else {
            newOption.val(result.ItemUnitID).html(result.PriUnit);
            $('#C_unit_name_' + dataid).append(newOption);
        }
    }
    else {

    }
}

// generate Price Table
function GeneratePriceTable(fnval) {
    $("#fnvalp").val(fnval);
    var cols = [];
    $("#Generated").find('tr').not(':first, .item_').each(function (rowIndex, r) {
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

// Submit Function JcSubmit
function SJSubmit(fnval) {
    var HTMLtbl = {
        getData: function (table) {
            var data = [];
            table.find('tr').not(':first').not('.item_').each(function (rowIndex, r) {
                var cols = {};
                $(this).find('input,select').each(function (colIndex, c) {
                    itid = $(this).attr('data-name');
                    itval = ($(this).val() != "") ? $(this).val() : $(this).text();
                    cols[itid] = itval;
                });
                data.push(cols);
            });
            return data;
        }
    }
    var data = HTMLtbl.getData($('#generateditem'));

    var CHTMLtbl = {
        getData: function (table) {
            var data = [];
            table.find('tr').not(':first').not('.Citem_').each(function (rowIndex, r) {
                var cols = {};
                $(this).find('input,select').each(function (colIndex, c) {
                    itid = $(this).attr('data-name');
                    itval = ($(this).val() != "") ? $(this).val() : $(this).text();
                    cols[itid] = itval;
                });
                data.push(cols);
            });
            return data;
        }
    }
    var Cdata = CHTMLtbl.getData($('#consumeditem'));

    var parameters = {};
    parameters.SJitems = data;
    parameters.CSJitems = Cdata;
    parameters.voucher = $('#Voucher').val();
    parameters.SJDate = $('#SJDate').val();
    parameters.Narration = $('#Narration').val();
    parameters.MCFrom = $('#ddlMCFrom').val();
    parameters.MCTo = $('#ddlMCTo').val();
    parameters.EmployeeId = $('#ddlEmployee').val();
    parameters.action = fnval;
    parameters.Ref1 = $('#Ref1').val();
    parameters.Ref2 = $('#Ref2').val();
    parameters.Ref3 = $('#Ref3').val();
    parameters.Ref4 = $('#Ref4').val();
    parameters.Ref5 = $('#Ref5').val();

    var url = "";
    if (fnval == "save") {
        url = $('#createForm')[0].action;
        $.ajax({
            async: false,
            cache: false,
            dataType: "json",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: url,
            data: JSON.stringify(parameters),
            beforeSend: function () {
                $("button").prop('disabled', true); // disable button
            },
            success: function (e) {

                if (e.status == true) {
                    if (fnval == "print") {
                        PrintJobCard(e);
                    } else {
                        $('.ajax_response', res_success).text(e.message);
                        $('.AlertDiv').prepend(res_success);
                    }
                    //setInterval(window.location.href = '/DummyJobCards/Index', 120);
                } else {
                    $('.ajax_response', res_danger).text(e.message);
                    $('.AlertDiv').prepend(res_danger);
                    $("button").prop('disabled', false); // enable button
                }
            }
        });
    }
    if (fnval == "update") {
        url = $('#sjeditForm')[0].action;
        $.ajax({
            async: false,
            cache: false,
            dataType: "json",
            type: "PUT",
            contentType: "application/json; charset=utf-8",
            url: url,
            data: JSON.stringify(parameters),
            beforeSend: function () {
                $("button").prop('disabled', true); // disable button
            },
            success: function (e) {
                if (e.status == true) {
                    if (fnval == "print") {
                        PrintJobCard(e);
                    } else {
                        $('.ajax_response', res_success).text(e.message);
                        $('.AlertDiv').prepend(res_success);
                    }
                    //setInterval(window.location.href = '/DummyJobCards/Index', 120);
                } else {
                    $('.ajax_response', res_danger).text(e.message);
                    $('.AlertDiv').prepend(res_danger);
                    $("button").prop('disabled', false); // enable button
                }
            }
        });
    }
    $.ajax({
        async: false,
        cache: false,
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: url,
        data: JSON.stringify(parameters),
        beforeSend: function () {
            $("button").prop('disabled', true); // disable button
        },
        success: function (e) {
            alert("alert");
            if (e.status == true) {
                if (fnval == "print") {
                    PrintJobCard(e);
                } else {
                    $('.ajax_response', res_success).text(e.message);
                    $('.AlertDiv').prepend(res_success);
                }
                setInterval(window.location.href = '/StockJournal/Index', 120);
            } else {
                $('.ajax_response', res_danger).text(e.message);
                $('.AlertDiv').prepend(res_danger);
                $("button").prop('disabled', false); // enable button
            }
        }
    });


    // JC Update Function

    function PrintJobCard(e) {
        $("#lblCardNo").text(e.summary.CardNo);
        $("#lblDate").text(convertToDate(e.summary.Date));
        $("#lblEmployee").text(e.summary.Employee);
        $("#lblMechName").text(e.summary.MechName);
        $("#lblPWCModel").text(e.summary.PWCModel);

        $("#lblDetails").text(e.summary.Details);

        $("#lbltrn").text(e.summary.TRN);
        // bind Party details
        $("#lblParty").text(e.summary.PartyName);
        var details = "";
        // var remark = e.summary.Details.replace(/\n/g, "<br/>");
        if (e.summary.Address != null) {
            details += e.summary.Address;
        }
        if (e.summary.City != null) {
            details += "<br />" + e.summary.City;
        }
        else if (e.summary.State != null) {
            details += "<br />" + e.summary.State;
        }
        else if (e.summary.Country != null) {
            details += "<br/>" + e.summary.Country;
        }
        else if (e.summary.Zip != null) {
            details += "<br />" + e.summary.Zip;
        }
        details += " <br/> Phone : ";
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
        $("[id$=lbladdress]").html(details);

        if (fmapp != null) {
            $.each(fmapp, function (i, mapp) {

                if (mapp.Field == "Ref1") {
                    $("#IblRef1").text(mapp.FieldName);
                    $("#IblRef1Val").text(e.Data.Ref1);
                    $("#divRef1").show();
                }
                if (mapp.Field == "Ref2") {
                    $("#IblRef2").text(mapp.FieldName);
                    $("#IblRef2Val").text(e.Data.Ref2);
                    $("#divRef2").show();
                }
                if (mapp.Field == "Ref3") {
                    $("#IblRef3").text(mapp.FieldName);
                    $("#IblRef3Val").text(e.Data.Ref3);
                    $("#divRef3").show();
                }
                if (mapp.Field == "Ref4") {
                    $("#IblRef4").text(mapp.FieldName);
                    $("#IblRef4Val").text(e.Data.Ref4);
                    $("#divRef4").show();
                }
                if (mapp.Field == "Ref5") {
                    $("#IblRef5").text(mapp.FieldName);
                    $("#IblRef5Val").text(e.Data.Ref5);
                    $("#divRef5").show();
                }
            });
        }

        var str2 = "";
        var count = 2;
        var str1 = "";
        var str3 = "";

        // bind items
        var itemsData = bindItem(e);


        $('#itemtable ').append(itemsData);


        var grt = parseFloat(e.summary.TotalAmount).toFixed(2);
        // bind total section
        var word = conNumber(grt);
        //if (e.summary.Discount > 0) {
        //    str2 += "<td>Discount خصم</td><td id='discountprint' class='text-right'>" + parseFloat(e.summary.Discount).toFixed(2) + "</td></tr> ";
        //    count++;
        //    str2 += "<tr class='border-top'><td>VAT<span style='direction:ltr'>(5.00%)</span> برميل</td><td  class='text-right'>" + parseFloat(e.summary.TaxAmount).toFixed(2) + "</td></tr>";
        //}
        //else {
        //    str2 += "<td>VAT<span style='direction:ltr'>(5.00%)</span> برميل </td><td class='text-right'>" + parseFloat(e.summary.TaxAmount).toFixed(2) + "</td></tr>";
        //}
        //if (type != "nobillsundry") {
        //    // bind bill sundry
        //    str2 += bindSundry(e);
        //    if (e.billsundry.length > 0) {
        //        count += e.billsundry.length;
        //    }
        //}
        var wordHtml = "<tr class='border-top'><td colspan='6' rowspan='2' style='padding-right: 20px;'><strong>" + word + " Only</strong></td><td>Amount كمية</td><td class='text-right'>" + parseFloat(e.summary.TotalAmount).toFixed(2) + "</td></tr>";
        str2 += "<tr class='border-top'><th>Total المبلغ الإجمالي(aed)</th><th class='text-right'>" + grt + "</th></tr>";

        //colspan='6' rowspan='" + count + "'
        //  str2 += "<tr class='border-top'><td colspan='8'><strong><u>Details :</u></strong><br/>" + remark + " </td></tr>";

        str1 = wordHtml + str2;
        $('#itemtable1').append(str1);

        var originalpage = document.body.innerHTML;
        var printContent = $('#printit').html();
        $('body').html(printContent);
        $('title').html(e.summary.CardNo);
        // find height

        var header = $(".print thead").height(); // default 265
        var items = $("#itemSection").height(); // default 558
        var itemstable = $("#itemtable").height();
        var terms = $("#itemtable1").height(); // default 137
        var footer = $("#footer").height(); // default 50
        var height = $(".print").height(); // total 
        if (terms > 137 && itemstable < 558) {
            //$('#container').css('min-height', '360px');
            //$('#container').attr('style','min-height:360px;other-styles');
        }

        window.print();
    }
    function bindItem(e) {
        var total = parseFloat(0);
        var str = "";
        var count = 1;
        $.each(e.item, function (i, item) {
            var subtot = parseFloat(item.ItemTotalAmount.toFixed(2));
            total += subtot;
            str += '<tr>';
            str += '<td>' + count + '</td>';
            str += '<td>' + item.ItemName + '</td>';
            str += '<td>' + item.ItemTotalAmount + '</td>';
            //str += '<td class="text-right">' + total.toFixed(2) + '</td>';
            str += '</tr>';
            count++;
        });
        return str;
    }
}


//function JcItemSubmit(fnval) {
//    var HTMLtbl = {
//        getData: function (table) {
//            var data = [];
//            table.find('tr').not(':first').not('.item_').each(function (rowIndex, r) {
//                var cols = {};
//                $(this).find('input,select').each(function (colIndex, c) {
//                    itid = $(this).attr('data-name');
//                    itval = ($(this).val() != "") ? $(this).val() : $(this).text();
//                    cols[itid] = itval;
//                });
//                data.push(cols);
//            });
//            return data;
//        }
//    }
//    var data = HTMLtbl.getData($('#itemtable'));

//    var parameters = {};
//    parameters.jcitems = data;
//    var url = $('#createForm')[0].action; //createForm

//    $.ajax({
//        async: false,
//        cache: false,
//        dataType: "json",
//        type: "POST",
//        contentType: "application/json; charset=utf-8",
//        url: url,
//        data: JSON.stringify(parameters),
//        beforeSend: function () {
//            $("button").prop('disabled', true); // disable button
//        },
//        success: function (e) {
//            if (e.status == true) {
//                $('.ajax_response', res_success).text(e.message);
//                $('.AlertDiv').prepend(res_success);
//                window.location.href = '/DummyJobCards/JobCardSetting';
//            } else {
//                $('.ajax_response', res_danger).text(e.message);
//                $('.AlertDiv').prepend(res_danger);
//                $("button").prop('disabled', false); // enable button
//            }
//        }
//    });
//}

//item pop up
function AddItemPopUp() {
    /* function for Create popup for item large */
    $('table').on('click', '.modal-create-lg', function (e) {
        e.preventDefault();
        var url = $(this).attr('href');
        modalshow(url, '#modal-create-lg');
        $('#KeepStock').prop('checked', true);
        $('#StockSection').show();
        // $("#SubUnitId").rules("remove", "required");
    });

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
        //var docUpload = $("#ItemDocument").get(0);
        //var docFiles = docUpload.files;
        //if (docFiles[0] != null) {
        //    formData.append(docFiles[0].name, docFiles[0]);
        //}

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
            setTimeout(function () { newWin.close(); }, 10);


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
    //$('div').on('click', '.btntaxAdd', function (e) {
    //    e.preventDefault();
    //    $(this).attr('data-target', '#modal-container-tax');
    //    $(this).attr('data-toggle', 'modal');
    //});
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
        var text = $("#TaxName").val();
        $('#TaxID option:selected').attr("selected", null);
        $('#HireTaxID option:selected').attr("selected", null);
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

                    var newOption = $('<option></option>');
                    newOption.val(data.Id).attr("selected", "selected");
                    newOption.html(text);
                    $('.form-control[name="HireTaxID"]').append(newOption)
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