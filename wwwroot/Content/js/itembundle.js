$('#KeepStock').click(function () {
    if ($('#KeepStock').prop('checked') == true) {
        $('#divstkqty').show();
        $('#StockQuantity').prop('required', true);
    }
    else {
        $('#divstkqty').hide();
          $('#StockQuantity').prop('required', false);
    }
});
$('#datewise').click(function () {
    if ($('#datewise').prop('checked') == true) {
        $('#divdate').show();
        $('#StartDate').prop('required', true);
        $('#EndDate').prop('required', true);
    }
    else {
        $('#divdate').hide();
        $('#StartDate').prop('required', false);
        $('#EndDate').prop('required', false);
    }
});

$('body').on('change', '#StartDate', function (e) {
    var seldate = $(this).val();
    
    var date = $(this).datepicker('getDate');

    if (date) {
        date.setDate(date.getDate() + 1);
    }
    $('#EndDate').datepicker("setDate", date);
    $('#EndDate').datepicker("setStartDate", date);
});


var count = 1, type = '';
limits = 500;
//Add Row
function addbundleitem(t, action, ItemUnit, ItemTax, ItemTotalAmount, ItemQuantity, Item, ItemCode, ItemName, ItemUnitPrice, ItemSubTotal, ItemWithCode, ItemTaxAmount, PurchasePrice, itemdata) {
    if (count == limits) alert("You have reached the limit of adding " + count + " inputs");
    else {
        var Option = "";
        var optionunit = "";
        var required = "";
        var slno = $('#addbundleitem tr').length + 1;
        var a = "item_name" + count,
        tabindex = count * 5;
        var row = "<tr class='item_' id='item_" + count + "'>";
        var data = "";
        var price = 0;
        var baseprice = 0;
        var mrp = 0;
        var htdata = "";
       
   
        tab1 = tabindex + 1;
        tab2 = tabindex + 2;
        tab3 = tabindex + 3;
        tab4 = tabindex + 4;
        tab5 = tabindex + 5;
        tab6 = tabindex + 6;

        if (Item != null) {
            row = "<tr class='item_" + Item + "' id='item_" + count + "'>";
            Option = "<option value='" + Item + "'>" + ItemWithCode + "</option>";
        }
        if (count == 1) {
            required = 'required="required"';
        }
        var chkread = "";
        if (action != '') {
            chkread = action == 'foredit' ? " readonly='readonly'" : "";
            type = action;
        }

        

      
        var inote = "";
        if (itemdata) {
            inote = itemdata.note;
        }
  
        
        if (itemdata) {
           
            price = itemdata.SellingPrice;
            baseprice = itemdata.BasePrice;
            mrp = itemdata.MRP;
           
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
        var itemaddbtn = "<span class='input-group-btn'><a type='button' href='/Item/AddItem' class='modal-create-lg btn btn-success btn-flat'><i class='fa fa-1x fa-plus-circle'></i></a></span>";

       
        data = "<td class='text-center'> " + slno + " </td>" +
                "<td class='input-group input-group-sm'><select data-name='ItemId' " + chkread + " class='form-control item_name' " + required + " data-id='" + count + "' placeholder='Item Name' id='item_name_" + count + "'  data-val-required='The Item field is required' onchange='GetItemdetails(this," + count + ",\"" + type + "\")'  >" + Option + "</select> " + itemaddbtn + "</td>" +
                "<td style='width:100px;'><select data-name='ItemUnit' class='form-control units unit_name_" + count + "' id='unit_name_" + count + "' " + required + " data-id='" + count + "' id='unit_name' onchange='unitchange(this," + count + ",\"" + type + "\");' "+chkread+"></select></td>" +
                "<td> <input data-name='ItemQuantity' type='number' name='product_quantity[]' onchange='quantity_change(" + count + ");' id='total_qntt_" + count + "' value='" + ItemQuantity + "'  class='total_qntt_" + count + " form-control text-right quty' placeholder='0' value='0' min='.01' tabindex='" + tab2 + "' "+chkread+" /></td>" +
                "<td><input data-name='ItemUnitPrice' type='number' name='product_rate[]' " + required + " onchange='rate_change(" + count + ",\"" + type + "\");' id='price_item_" + count + "' value='" + ItemUnitPrice + "' class='price_item_" + count + " form-control text-right sell_price' placeholder='0.00' min='0' tabindex='" + tab3 + "' readonly='readonly'/><input type='hidden' data-value='" + price + "' value='" + baseprice + "' name='base_rate' id='base_rate_" + count + "'> </td> " +
                "<td><input data-name='ItemSubTotal' type='number' name='sub_total[]' id='sub_total_" + count + "' class='sub_total_" + count + " form-control text-right subtotal' value='" + ItemSubTotal + "'   placeholder='0.00' min='0' tabindex='" + tab3 + "' readonly='readonly'/></td>" +

                "<td><input type='text' id='tax_" + count + "' class='form-control text-right tax tax_" + count + "' tabindex='" + tab4 + "' readonly='readonly' /><input type='hidden' class='item_amount' name='item_amount' id='item_amount_" + count + "'/><input data-name='ItemTaxAmount' type='hidden' class='tot_tax' name='tot_tax' id='tot_tax_" + count + "'/><input type='hidden' data-name='ItemTax' class='tax_percentage' value='" + ItemTax + "' name='tax_percentage' id='tax_percentage_" + count + "'/></td> " +
                "<td class='text-right'><input data-name='ItemTotalAmount' class='total_price total_price_" + count + " form-control text-right' type='text' name='total_price[]' value='" + ItemTotalAmount + "' id='total_price_" + count + "' value='0.00' readonly='readonly'/><input type='hidden' class='cfactor' name='cfactor' id='cfactor_" + count + "'/></td>" +
                "<td class='text-center'><button " + chkread + " tabindex='" + tab5 + "' style='text-align: right;' class='btn btn-danger' type='button' value='Delete' onclick='deleteRow(this)'><i class='fa fa-trash fa-1x'></i></button>" + htdata + " <input type='hidden' value='" + PurchasePrice + "' class='purprice' name='purprice' id='purprice_" + count + "'/></td>";
        row += data + "</tr>";
        $('#' + t).append(row);
        // $('#item_ .item_name').focus();
        searchItem();
        if (itemdata) {
            rate_change(count, type, 'foredit');
            createUnitList(itemdata, count);
        }
        else
            rate_change(count, type);
        count++;
        setTabIndex();
    }
}



//item details
function GetItemdetails(selectObject, dataid, action) {    
    if (selectObject.value) {
        var ItemId = selectObject.value;
        if (ItemId != null) {
            if ($(".item_" + ItemId).length > 1) {
                if ($(".item_" + ItemId).length == 2) {
                    alert("Cannot Add an Item More than 2 times !!");
                    $("#item_name_" + dataid).val(null).trigger("change");
                }
            } else {
                if ($(".item_" + ItemId).length > 0) {
                    if (confirm('Are you sure want to Add this item Again?')) {
                        itemUpdate(selectObject, dataid, action);
                    }
                    else {
                        $("#item_name_" + dataid).val(null).trigger("change");
                    }
                }
                else {
                    itemUpdate(selectObject, dataid, action);
                }
            }

        }
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
           // if ((result.KeepStock != true) || (result.KeepStock == true && result.total > 0)) {
                // append item unit list
                createUnitList(result, dataid);
              
                $(".price_item_" + dataid).val(result.SellingPrice);
                $("#item_amount_" + dataid).val(result.SellingPrice);
                $("#base_rate_" + dataid).attr("data-value", result.SellingPrice);
               
                $("#purprice_" + dataid).val(result.PurchasePrice);


                $("#total_qntt_" + dataid).val(1);
                $("#tax_percentage_" + dataid).val(result.Tax);
                $("#base_rate_" + dataid).val(result.BasePrice);

                $("#cfactor_" + dataid).val(result.ConFactor);

                rowSubTotal(dataid);
                CalculatetblItemListSum();
              
                $(selectObject).closest('tr').attr('class', "item_" + result.ItemID);
                
                 //minstockupdate(result, dataid);
               
                if ($(".item_").length == 0) {
                    addbundleitem('addbundleitem', '', '', '0.00', '0.00', '0');
                }
                $('.unit_name_' + dataid).focus();
            //}
            //else {
            //    alert("This Item is Out of Stock!!!");
            //    var classname = $($("#total_qntt_" + dataid)).closest('tr').attr('class');
            //    if (classname != 'item_') {
            //        $("." + classname + " .btn-danger").click();
            //    }
            //    else {
            //        $("#item_name_" + dataid).val(null).trigger("change");
            //    }
            //}
        }
    });
}
//check minimum stock
//function minstockupdate(result, dataid) {
//    var htdata = "<div class='minstock_" + dataid + "'";
//    if (result.KeepStock == true) {
//        totalstock = result.total;
//        minstock = result.MinStock * result.ConFactor;
       
//        htdata += " data-keeps='yes' data-min='" + minstock + "' data-confactor='" + result.ConFactor + "' data-stock='" + totalstock + "'>";
//    }
//    else {
//        htdata += " data-keeps='no' >";
//    }
//    if ($(".minstock_" + dataid).length) {
//        $(".minstock_" + dataid).remove();
//    }
//    $('#item_' + dataid).append(htdata);
//}
//function minstockcheck(arg) {
//    var keepstock = $(".minstock_" + arg).attr('data-keeps');
//    if (keepstock == "yes") {
//        var index = $('#unit_name_' + arg).prop('selectedIndex');
//        var unitname = $('#unit_name_' + arg).find('option:selected').text();
//        var minstock = parseFloat($(".minstock_" + arg).attr('data-min'));
//        var confactor = parseFloat($(".minstock_" + arg).attr('data-confactor'));
//        var stock = parseFloat($(".minstock_" + arg).attr('data-stock'));
//        var quantity = parseFloat($(".total_qntt_" + arg).val());
//        var qty = 0;
//        var classn = $("#item_" + arg).attr('class');
//        $("." + classn).each(function () {
//            var rowid = $(this).attr('id');
//            var arr = rowid.split('_');
//            var arg1 = arr[1];
//            var index1 = $("#" + rowid + " .units").prop('selectedIndex');
//            var curent = $("#" + rowid + " .quty").val();
//            var confactor1 = parseFloat($("#" + rowid + "  .minstock_" + arg1).attr('data-confactor'));
//            if (index == 0) {
//                qty += (curent * confactor1);
//            }
//            else {
//                qty += curent;
//            }
//        });
//        if (index == 0) {
//            stock = stock - (qty - quantity);
//            minstock = minstock / confactor;
//            stock = stock / confactor;
//            var tostock = stock - quantity;
//            var totstock = tostock / confactor;

//            //var totstock = stock - qty;
//            if (totstock <= minstock && totstock >= 0) {
//                alert("Stock Exceeds Minimum Stock");
//            }
//            if (totstock < 0) {
//                stock = stock.toFixed(2);
//                alert("This Item Is Going To Out of Stock!!! Only " + stock + " " + unitname + "Items Are Available In Stock..");
//                $(".total_qntt_" + arg).val(parseInt(stock));
//            }
//        } else {
//            stock = stock - (qty - quantity);
//            var totstock = stock - quantity;
//            if (totstock <= minstock && totstock >= 0) {
//                alert("Stock Exceeds Minimum Stock");
//            }
//            if (totstock < 0) {
//                alert("This Item Is Going To Out of Stock!!! Only " + stock + " " + unitname + " Items Are Available In Stock..");
//                $(".total_qntt_" + arg).val(stock);
//            }
//        }
//    }
//}
// create units based on primary and secondary
function createUnitList(result, dataid) {
    if ($("#BItemExist").val() == "false")
    {        
        $("#unit_name_" + dataid).attr('readonly', false);
        $("#total_qntt_" + dataid).attr('readonly', false);        
    }
    // clear previous content
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

    $(".item_name").select2({
        placeholder: 'Search Item by Name or Code',
        minimumInputLength: 0,
        ajax: {
            // url: "/ItemBundle/SearchItem",
            url: "/Item/Searchdetails",
            dataType: 'json',
            type: "POST",
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    ItemID: selecteditem,
                    page: params.page || 1,
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
    rate_change(count);
}


function quantity_change(arg) {
    if ($('#item_name_' + arg).val() != null) {
    //minstockcheck(arg);
    rowSubTotal(arg);
    CalculatetblItemListSum();
    }
    else {
        $('#total_qntt_'+ arg).val(0);
    }

}
function rate_change(arg, type, foredit) {
    //minstockcheck(arg);
    var baserate = $("#base_rate_" + arg).val();
    var rate = $(".price_item_" + arg).val();

    if (parseFloat(baserate) > parseFloat(rate) && parseFloat(rate) > 0 && foredit != 'foredit') {
        alert("Selling price is less than Base Price ");
    }

    rowSubTotal(arg);
    CalculatetblItemListSum();
}



function rowSubTotal(arg) {

    var tax = $("#tax_percentage_" + arg).val();
    var quantity = $(".total_qntt_" + arg).val();
    var rate = $(".price_item_" + arg).val();
    var subtotal = quantity * rate;
    $(".sub_total_" + arg).val(subtotal.toFixed(2));


    var taxAmount = subtotal * (tax / 100);
    var Total = subtotal + taxAmount;

    $("#tot_tax_" + arg).val(taxAmount.toFixed(2));
    $(".tax_" + arg).val(taxAmount.toFixed(2) + " (" + tax + "%)");
    $(".total_price_" + arg).val(Total.toFixed(2));
}
function CalculatetblItemListSum() {

    //alert($("#tot_tax_1").val());

    var tax = $(".tot_tax").val();
    var qty = $(".quty").val();
    if (tax > 0 || qty != 0) {
        var tbody = $("#bundletable tbody");
        if (tbody.children().length > 0) {
            var gtTax = 0;
            var gtTotal = 0;
            var gtQty = 0;
            var gtSubTotal = 0;
            var SPrice = 0;
            var PPrice = 0;
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


            $(".sell_price").each(function () {
                var subTot = this.value;
                subTot = subTot || 0.00;
                SPrice = parseFloat(SPrice) + parseFloat(subTot);
            });
            SPrice = SPrice || 0.00;


            $(".purprice").each(function () {
                var subTot = parseFloat(this.value);
                subTot = subTot || 0.00;
                PPrice += subTot;
            });
            PPrice = PPrice || 0.00;



            // $("#GrandTotal").val(parseFloat(gtTotal).toFixed(2));
            $("[id$=TotRate]").text((gtRate).toFixed(2));
            $("[id$=total]").text((gtTotal).toFixed(2));
            $("[id$=ItemCount]").val(tbody.children().length);
            $("[id$=ItemQty]").text((gtQty).toFixed(2));
            $("[id$=SubTotal]").text((gtSubTotal).toFixed(2));

            $("#ActualPrice").text((SPrice).toFixed(2));
            $("#ActualCost").val((PPrice).toFixed(2));

            $("#SellingPrice").val((SPrice).toFixed(2));


        }
    } else {
        $("[id$=TotRate]").text("");
        $("[id$=total]").text("");
        $("[id$=ItemCount]").val("");
        $("[id$=ItemQty]").text("");
        $("[id$=SubTotal]").text("");
        $("#ActualPrice").text("");
        $("#ActualCost").val("");
        $("#SellingPrice").val("");
        $("#total_tax_amount").text("");
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
    $('#addbundleitem tr').each(function () {
        $(this).find('td:first').text(i);
        i++;
    });
    CalculatetblItemListSum();
}

function BundleSubmit(fnval,formv) {


    var formData = new FormData(formv);
    var imgUpload = $("#ImgFileName").get(0);
    var imgFiles = imgUpload.files;
    //if (imgFiles[0] != null) {
    //    formData.append("ImgFileName", imgFiles[0]);
    //}

    //var HTMLtbl = {
    //    getData: function (table) {
    //        var data = new FormData();
    //        //var data = [];
    //        table.find('tr').not(':first').not('.item_').each(function (rowIndex, r) {
    //            //var cols = {};
    //            $(this).find('input,select').each(function (colIndex, c) {
    //                itid = $(this).attr('data-name');
    //                itval = ($(this).val() != "") ? $(this).val() : $(this).text();
    //               // cols[itid] = itval;
    //                data.append(itid + "[]", itval);
    //            });
    //           // data.push(cols);
    //        });
    //        return data;
    //    }
    //}
      //var parameters = {};
      //parameters.ItemBundleId = $('#ItemBundleId').val();
      //parameters.ItemName = $('#ItemName').val();
      //parameters.ItemCode = $('#ItemCode').val();
      //parameters.Barcode = $('#Barcode').val();
      //parameters.ActualCost = $('#ActualCost').val();
      //parameters.ActualPrice = $('#ActualPrice').text();
      //parameters.SellingPrice = $('#SellingPrice').val();
      //parameters.KeepStock = $('#KeepStock').val();
      //parameters.StockQuantity = $('#StockQuantity').val();
      //parameters.ItemCategoryID = $('#ddlCategory').val();
      //parameters.TaxID = $('#ddlTax').val();
      //parameters.Note = $('#Note').val();
      //parameters.BundleType = $('#BundleType').val();

      //parameters.StartDate = $('#StartDate').val();
      //parameters.EndDate = $('#EndDate').val();

    //var data = HTMLtbl.getData($('#bundletable'));

     //parameters.bundleitem = data;
    var bundleId=$('#ItemBundleId').val();
    
    if (bundleId != null && bundleId != "undefined") {
        formData.append("ItemBundleId", bundleId);
    }
    
     //formData.append("ItemName", $('#ItemName').val());
     //formData.append("ItemCode", $('#ItemCode').val());
     //formData.append("ActualCost", $('#ActualCost').val());
     //formData.append("ActualPrice", $('#ActualPrice').text());
     //formData.append("SellingPrice", $('#SellingPrice').val());
     //formData.append("KeepStock", $('#KeepStock:checked').val() || false);
     //formData.append("StockQuantity", $('#StockQuantity').val());
     //formData.append("ItemCategoryID", $('#ddlCategory').val());
     //formData.append("TaxID", $('#ddlTax').val());
     //formData.append("Note", $('#Note').val());
     //formData.append("BundleType", $('#BundleType').val());

     //formData.append("StartDate", $('#StartDate').val());

     //formData.append("bundleitem", JSON.stringify(data));
     //formData.append("bundleitem", data);
     $('#bundletable').find('tr').not(':first').not('.item_').each(function (rowIndex, r) {
         $(this).find('input,select').each(function (colIndex, c) {
             itid = $(this).attr('data-name');
             itval = ($(this).val() != "") ? $(this).val() : $(this).text();
             if (typeof itid !== "undefined") {
                 if ((itid == 'ItemUnit') && (itval == null)) {

                 } else {
                     formData.append("bundleitem[" + rowIndex + "]." + itid, itval);
                 }
             }
         });
     });
     //formData.bundleitem=data;

    var url = "";
    if (fnval == "save") {
        url = $('#createForm')[0].action;//"/ItemBundle/Create";
    }
    if (fnval == "update") {
        url = $('#updateForm')[0].action;//"/ItemBundle/Edit";
    }
    $.ajax({
        async: true,
        cache: false,
        dataType: "json",
        type: "POST",
        //contentType: "application/json; charset=utf-8",
        processData: false,
        contentType: false,
        url: url,
        data: formData,//JSON.stringify(parameters),
        beforeSend: function () {
            $("button").prop('disabled', true); // disable button
        },
        success: function (e) {
            //for (var pair of formData.entries()) {
            //    console.log(pair[0] + ', ' + pair[1]);
            //}
            if (e.status) {
                $('.ajax_response', res_success).text(e.message);
                $('.AlertDiv').prepend(res_success);
                window.location.href = '/ItemBundle/Index';
            }
            else {
                $('.ajax_response', res_danger).text(e.message);
                $('.AlertDiv').prepend(res_danger);
            }
            $("button").prop('disabled', false); // enable button
        }
    });

}