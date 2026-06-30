var count = 1, type = '';
limits = 500;
//Add Row 
var pcount = 2;
                                                    
function addrowrtn(t, action, ItemUnit, ItemTax, ItemTotalAmount, ItemQuantity, Item, ItemCode, ItemName, ItemUnitPrice, ItemSubTotal, ItemWithCode, ItemTaxAmount, ItemDiscount, ItemNote, itemdata, ConFactor, ItemCategory, ItemBrand, PPRICE, MRP, SellPrice, BPrice, tag1, tag2, tag3, tag4, tag5, prefix, division, supplier) {
    if (count == limits) alert("You have reached the limit of adding " + count + " inputs");
    else {
        var Option = "";
        var optionunit = "";
        var required = "";
        var slno = $('#addinvoiceItempopup tr').length + 1;
        var a = "ritem_name" + count,
        tabindex = count * 5;
        var row = "<tr class='ritem_' id='ritem_" + count + "'>";
        var data = "";
        var price = 0;
        var baseprice = 0;
        var mrp = 0;
        var htdata = "";
        var itemnote = "";
        var notbtn = "";
        var divid = "ritem_name_" + Item;
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
            row = "<tr class='ritem_" + Item + "' id='ritem_" + count + "'>";
            Option = "<option value='" + Item + "'>" + ItemWithCode + "</option>";
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
        itemnote = '<div id="rmodal-item-' + count + '" class="modal fade" role="dialog" aria-hidden="true"><div class="modal-dialog"><div class="modal-content">' +
            '<div class="form-group"><textarea name="ritemnote" cols="40" rows="10" class="form-control ritemnote" id="ritemnote-' + count + '" maxlength="1000">' + inote + '</textarea></div>' +
            '<div class="form-group"><button class="btn btn-info" type="button" data-dismiss="modal">Save</button></div>' +
            '</div></div></div>';
        notbtn = "<button type='button' class='ritnote btn btn-default btn-flat' data-toggle='modal' data-target='#rmodal-item-" + count + "'><i class='fa fa-1x fa-file-text-o'></i></button>";

        if (itemdata) {
            if (type == "purchase")
                price = itemdata.PurchasePrice;
            else
                price = itemdata.SellingPrice;
            baseprice = itemdata.BasePrice;
            mrp = itemdata.MRP;
            if (type == "sales") {
                htdata = "<div class='rminstock_" + count + "'";
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
                "<td class='input-group input-group-sm'><select class='form-control ritem_name' " + required + " data-id='" + count + "' placeholder='Item Name' id='ritem_name_" + count + "'  data-msg-required='The Item field is required' onchange='GetItemdetailsR(this," + count + ",\"" + type + "\")'>" + Option + "</select> " + itemaddbtn + "</td>" +
                "<td style='width:100px;'>Unit<select class='form-control runits runit_name_" + count + "' id='runit_name_" + count + "' data-id='" + count + "' id='runit_name' onchange='unitchangeR(this," + count + ",\"" + type + "\"); '></select></td>" +
                "<td>Qty <input type='number' name='product_quantity[]' data-msg-min ='The Item Quantity must be Greater than Zero' onchange='quantity_changeR(" + count + ");' id='rtotal_qntt_" + count + "' value='" + ItemQuantity + "'  class='rtotal_qntt_" + count + " form-control text-right rquty' placeholder='0' value='0' min='.01' tabindex='" + tab2 + "'/></td>" +
                "<td>Rate<input type='number' name='product_rate[]' " + required + " data-msg-required='The Item Rate field is required' onchange='rate_changeR(" + count + ",\"" + type + "\");' id='rprice_item_" + count + "' value='" + ItemUnitPrice + "' class='rprice_item_" + count + " form-control text-right rtotrate' placeholder='0.00' min='0' tabindex='" + tab3 + "'/><input type='hidden' data-value='" + price + "' value='" + baseprice + "' name='base_rate' id='rbase_rate_" + count + "'> </td> " +
                "<td>S.Total<input type='number' name='sub_total[]' id='rsub_total_" + count + "' class='rsub_total_" + count + " form-control text-right rsubtotal' value='" + ItemSubTotal + "'   placeholder='0.00' min='0' tabindex='" + tab3 + "' readonly='readonly'/></td>" +
                "<td>Discount<input type='number' name='item_discount[]' id='ritem_discount" + count + "' onchange='itemdiscount_changeR(" + count + ");' class='ritem_discount" + count + " form-control text-right ritem_discount' value='" + ItemDiscount + "' value='0.00' placeholder='0.00' tabindex='" + tab3 + "'/></td>" +
                "<td>Tax<input type='text' id='rtax_" + count + "' class='form-control text-right rtax rtax_" + count + "' tabindex='" + tab4 + "' readonly='readonly' /><input type='hidden' class='ritem_amount' name='item_amount' id='ritem_amount_" + count + "'/><input type='hidden' class='rtot_tax' name='tot_tax' id='rtot_tax_" + count + "'/><input type='hidden'  class='rtax_percentage' value='" + ItemTax + "' name='tax_percentage' id='rtax_percentage_" + count + "'/></td> " +
                "<td class='text-right'>Total Price<input class='rtotal_price rtotal_price_" + count + " form-control text-right' type='text' name='total_price[]' value='" + ItemTotalAmount + "' id='rtotal_price_" + count + "' value='0.00' readonly='readonly'/><input type='hidden' class='rcfactor' value='" + ConFactor + "' name='cfactor' id='rcfactor_" + count + "'/> " +
                "<input type='hidden' class='rselitem_name_" + count + "' value='" + ItemName + "' name='selitem_name' id='rselitem_name_" + count + "'/> " +
                "<input type='hidden' class='rselitem_code_" + count + "' value='" + ItemCode + "' name='selitem_code' id='rselitem_code_" + count + "'/> " +
                "<input type='hidden' class='rselitem_category_" + count + "' value='" + ItemCategory + "' name='selitem_category' id='rselitem_category_" + count + "'/> " +
                "<input type='hidden' class='rselitem_brand_" + count + "' value='" + ItemBrand + "' name='selitem_brand' id='rselitem_brand_" + count + "'/> " +
                "<input type='hidden' class='rselitem_pprice_" + count + "' value='" + PPRICE + "' name='selitem_pprice' id='rselitem_pprice_" + count + "'/> " +
                "<input type='hidden' class='rselitem_pmrp_" + count + "' value='" + MRP + "' name='selitem_pmrp' id='rselitem_pmrp_" + count + "'/> " +
                "<input type='hidden' class='rselitem_psprice_" + count + "' value='" + SellPrice + "' name='selitem_psprice' id='rselitem_psprice_" + count + "'/> " +
                "<input type='hidden' class='rselitem_pbprice_" + count + "' value='" + BPrice + "' name='selitem_pbprice' id='rselitem_pbprice_" + count + "'/> " +
                "<input type='hidden' class='rtagline1_" + count + "' value='" + tag1 + "'  name='tagline1' id='rtagline1_" + count + "'/> " +
                "<input type='hidden' class='rtagline2_" + count + "' value='" + tag2 + "'  name='tagline2' id='rtagline2_" + count + "'/> " +
                "<input type='hidden' class='rtagline3_" + count + "' value='" + tag3 + "'  name='tagline3' id='rtagline3_" + count + "'/> " +
                "<input type='hidden' class='rtagline4_" + count + "' value='" + tag4 + "' name='tagline4' id='rtagline4_" + count + "'/> " +
                "<input type='hidden' class='rtagline5_" + count + "' value='" + tag5 + "' name='tagline5' id='rtagline5_" + count + "'/> " +
                "<input type='hidden' class='rprefix_" + count + "' value='" + prefix + "' name='prefix' id='rprefix_" + count + "'/> " +
                "<input type='hidden' class='rdivision_" + count + "' value='" + division + "' name='division' id='rdivision_" + count + "'/> " +
                "<input type='hidden' class='rsupplier_" + count + "' value='" + supplier + "' name='supplier' id='rsupplier_" + count + "'/> " +
                "</td>" +
                "<td class='text-center'><button tabindex='" + tab5 + "' style='text-align: right;' class='btn btn-danger' type='button' value='Delete' onclick='deleteRowR(this)'><i class='fa fa-trash fa-1x'></i></button>" + itemnote + htdata + "</td>";
        row += data + "</tr>";
        $('#' + t).append(row);
        // $('#item_ .item_name').focus();
        searchItemR();
        if (itemdata) {
            rate_changeR(count, type, 'foredit');
            createUnitListR(itemdata, count, action);
        }
        else
            rate_changeR(count, type);
        count++;
        setTabIndexR();
    }
}



//item details
function GetItemdetailsR(selectObject, dataid, action) {
    if (selectObject.value) {
        var ItemId = selectObject.value;
        if (ItemId != null) {
            if ($(".ritem_" + ItemId).length > 0) {
                if ($(".ritem_" + ItemId).length < 4) {
                    if (confirm('Are you sure want to Add this item Again?')) {
                        itemUpdateR(selectObject, dataid, action);
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
                itemUpdateR(selectObject, dataid, action);
            }
        }
    }
}
// update item details
function itemUpdateR(selectObject, dataid, action) {
    var mc = $("#ddlMC").val();
    if (action == "sales" || action == "quot") {
        var ROnlyRate = $("#ROnlyRate").val();
        if (ROnlyRate == "active") {
            $("#rprice_item_" + dataid).attr('readonly', true);
        }
    }
    var Stype = $('#SaleType').val();
    var Hire = $('#ddlHType').val();


    var newUrl;
    if (mc != null && mc > 0 && Stype == 1) {
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
            createUnitListR(result, dataid, action);
            if (action == "sales" || action == "quot") {
                //if ($('#SaleType').val() == 2) {
                //    var Stype = $('#ddlHType').val();
                //    var Item = $("#item_name_" + dataid).val();
                //    $.ajax({
                //        url: '/HireType/GetHireRatebyTypeAndId',
                //        data: { hiretype: Stype, item: Item },
                //        type: "POST",
                //        dataType: "JSON",
                //        success: function (result) {
                //            $(".price_item_" + dataid).val(result);
                //            $("#item_amount_" + dataid).val(result);
                //            $("#base_rate_" + dataid).attr("data-value", result);

                //        }
                //    });

                //}
                //else {
                $(".rprice_item_" + dataid).val(result.SellingPrice);
                $("#ritem_amount_" + dataid).val(result.SellingPrice);
                $("#rbase_rate_" + dataid).attr("data-value", result.SellingPrice);
                //}
            }
            if (action == "purchase") {
                $(".rprice_item_" + dataid).val(result.PurchasePrice);
                $("#ritem_amount_" + dataid).val(result.SellingPrice);
                $("#rbase_rate_" + dataid).attr("data-value", result.PurchasePrice);
            }
            $("#rtotal_qntt_" + dataid).val(1);
            var SalesType = $("#SalesType").val() || "";
            var PurchaseType = $("#PurchaseType").val() || "";
            var TaxP = result.Tax;
            if (PurchaseType == 2 || SalesType == 2) {
                TaxP = 0;
            }
            $("#rtax_percentage_" + dataid).val(TaxP);
            $("#rbase_rate_" + dataid).val(result.BasePrice);
            $("#rcfactor_" + dataid).val(result.ConFactor);
            rowSubTotalR(dataid);
            CalculatetblItemListSumR();
            grandtotalcalculationR();
            paidamountcalculationR();

            if (action != "sales" || (action == "sales" && result.KeepStock != true) || (action == "sales" && result.KeepStock == true && result.total > 0)) {
                // append item unit list 

                $(selectObject).closest('tr').attr('class', "ritem_" + result.ItemID);
                if (action == "sales") {
                    minstockupdateR(result, dataid);
                }
                if ($(".ritem_").length == 0) {
                    addrowrtn('addinvoiceItempopup', '', '', '0.00', '0.00', '0');
                }
                $('.runit_name_' + dataid).focus();
            }
            else if ((result.KeepStock == true && result.CheckStock == 0 && result.total <= 0)) {

                var res = confirm("Are you Sure Want To Add Items In Less Stock ?");
                if (res == true) {
                    $(selectObject).closest('tr').attr('class', "ritem_" + result.ItemID);
                    if (action == "sales") {
                        minstockupdateR(result, dataid);
                    }
                    if ($(".ritem_").length == 0) {
                        addrowrtn('addinvoiceItempopup', '', '', '0.00', '0.00', '0');
                    }
                    $('.runit_name_' + dataid).focus();
                }
                else {
                    $("#ritem_name_" + dataid).val(null).trigger("change");
                    $("#runit_name_" + dataid).val(null).trigger("change");
                    $("#rtotal_qntt_" + dataid).val(null).trigger("change");
                    $("#rprice_item_" + dataid).val(null).trigger("change");
                }
            }
            else {
                alert("This Item is Out of Stock!!!");
                var classname = $($("#rtotal_qntt_" + dataid)).closest('tr').attr('class');
                if (classname != 'ritem_') {
                    $("." + classname + " .btn-danger").click();
                }
                else {
                    $("#ritem_name_" + dataid).val(null).trigger("change");
                    $("#runit_name_" + dataid).val(null).trigger("change");
                    $("#rtotal_qntt_" + dataid).val(null).trigger("change");
                    $("#rprice_item_" + dataid).val(null).trigger("change");
                }
            }
        }
    });
}
function minstockupdateR(result, dataid) {
    var htdata = "<div class='rminstock_" + dataid + "'";
    if (result.KeepStock == true) {
        totalstock = result.total;
        minstock = result.MinStock * result.ConFactor;

        htdata += " data-keeps='yes' data-min='" + minstock + "' data-confactor='" + result.ConFactor + "' data-stock='" + totalstock + "'>";
    }
    else {
        htdata += " data-keeps='no' >";
    }
    if ($(".rminstock_" + dataid).length) {
        $(".rminstock_" + dataid).remove();
    }
    $('#ritem_' + dataid).append(htdata);
}
function minstockcheckR(arg) {
    var keepstock = $(".rminstock_" + arg).attr('data-keeps');
    if (keepstock == "yes") {
        var index = $('#runit_name_' + arg).prop('selectedIndex');
        var unitname = $('#runit_name_' + arg).find('option:selected').text();
        var minstock = parseFloat($(".rminstock_" + arg).attr('data-min'));
        var confactor = parseFloat($(".rminstock_" + arg).attr('data-confactor'));
        var stock = parseFloat($(".rminstock_" + arg).attr('data-stock'));
        var quantity = parseFloat($(".rtotal_qntt_" + arg).val());

        var qty = 0;
        var classn = $("#ritem_" + arg).attr('class');

        $("." + classn).each(function () {

            var rowid = $(this).attr('id');
            var arr = rowid.split('_');
            var arg1 = arr[1];
            var index1 = $("#" + rowid + " .runits").prop('selectedIndex');
            var curent = $("#" + rowid + " .rquty").val();
            var confactor1 = parseFloat($("#" + rowid + "  .rminstock_" + arg1).attr('data-confactor'));
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

            //var totstock = stock - qty;
            if (totstock <= minstock && totstock >= 0) {
                alert("Stock Exceeds Minimum Stock");
            }
            else if (quantity >= stock && stock <= 0) {
                $(".rtotal_qntt_" + arg).val(quantity);
                stock = stock - (qty - quantity);
            }
            else if (totstock < 0) {
                stock = stock.toFixed(2);
                alert("This Item Is Going To Out of Stock!!! Only " + stock + " " + unitname + "Items Are Available In Stock..");
                $(".rtotal_qntt_" + arg).val(parseInt(stock));
            }

        } else {
            stock = stock - (qty - quantity);
            var totstock = stock - quantity;
            if (totstock <= minstock && totstock >= 0) {
                alert("Stock Exceeds Minimum Stock");
            }
            if (totstock < 0) {
                alert("This Item Is Going To Out of Stock!!! Only " + stock + " " + unitname + " Items Are Available In Stock..");
                $(".rtotal_qntt_" + arg).val(stock);
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
function createUnitListR(result, dataid, action) {
    // clear previous content
    if (action == "sales" || action == "quot" || action == "foredit") {
        var ROnlyRate = $("#ROnlyRate").val();
        if (ROnlyRate == "active") {
            $("#rprice_item_" + dataid).attr('readonly', true);
        }
    }
    $('#runit_name_' + dataid).empty();
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

            $('#runit_name_' + dataid).append(newOption);
            $('#runit_name_' + dataid).append(newOption1);
        }
        else {
            newOption.val(result.ItemUnitID).html(result.PriUnit);
            $('#runit_name_' + dataid).append(newOption);
        }
    }
    else {

    }
}
// search item
function searchItemR() {
    var selecteditem = new Array();

    $(".ritem_name").each(function () {
        selecteditem.push($(this).val());
    });
    var mc = $("#ddlMC").val();
    if (mc != null && mc > 0) {
        $(".ritem_name").select2({
            placeholder: 'Search Item by Code',
            minimumInputLength: 0,
            ajax: {
                url: "/Item/SearchdetailsMC",
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
        $(".ritem_name").select2({
            placeholder: 'Search Item by Code',
            minimumInputLength: 0,
            ajax: {
                url: "/Item/Searchdetails",
                dataType: 'json',
                type: "POST",
                delay: 50,
                data: function (params) {
                    return {
                        q: params.term || "",
                        cust: $("#ddlCustomer").val(),
                        ItemID: selecteditem,
                        page: params.page || 1,
                        constat : $("#ContType").val()
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
    rate_changeR(count);
}

function repoFormatResultR(repo) {
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

function repoFormatSelectionR(repo) {
    return repo.text;
}

function quantity_changeR(arg) {

    //if ($('total_qntt_' + arg).val() > 0) {
    minstockcheckR(arg);
    rowSubTotalR(arg);
    CalculatetblItemListSumR();
    grandtotalcalculationR();
    paidamountcalculationR();
}
function rate_changeR(arg, type, foredit) {
    minstockcheckR(arg);
    var baserate = $("#rbase_rate_" + arg).val();
    var rate = $(".rprice_item_" + arg).val();

    if (parseFloat(baserate) > parseFloat(rate) && type == 'sales' && parseFloat(rate) > 0 && foredit != 'foredit') {
        alert("Selling price is less than Base Price ");
    }

    rowSubTotalR(arg);
    CalculatetblItemListSumR();
    grandtotalcalculationR();
    paidamountcalculationR();
}
function itemdiscount_changeR(arg) {
    rowSubTotalR(arg);
    CalculatetblItemListSumR();
    grandtotalcalculationR();
    paidamountcalculationR();
}

//function discount_change(arg) {
//    grandtotalcalculation();
//}
function paidamount_changeR() {
    //CalculatetblItemListSum();
    paidamountcalculationR();
}

function rowSubTotalR(arg) {
    var tax = $("#rtax_percentage_" + arg).val();
    var quantity = $(".rtotal_qntt_" + arg).val();
    var rate = $(".rprice_item_" + arg).val();
    //alert("price : " + rate + " arg =" + arg);
    var subtotal = quantity * rate;
    $(".rsub_total_" + arg).val(subtotal.toFixed(2));
    var itemdiscount = $(".ritem_discount" + arg).val();
    subtotal = subtotal - itemdiscount;

    var taxAmount = subtotal * (tax / 100);
    var Total = subtotal + taxAmount;

    $("#rtot_tax_" + arg).val(taxAmount.toFixed(2));
    $(".rtax_" + arg).val(taxAmount.toFixed(2) + " (" + tax + "%)");
    $(".rtotal_price_" + arg).val(Total.toFixed(2));
}

function paidamountcalculationR() {
    var paidAmt = $("#SRPaidAmount").val();
    var gdTotal = $("#SRGrandTotal").val();
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
function CalculatetblItemListSumR() {
    var tax = $(".tot_tax").val();
    var qty = $(".ItemQty").val();
    if (tax > 0 || qty != 0) {
        var tbody = $("#normalinvoicez tbody");
        if (tbody.children().length > 0) {
            var gtTax = 0;
            var gtTotal = 0;
            var gtQty = 0;
            var gtSubTotal = 0;
            var gtDiscount = 0;
            var gtRate = 0;
            $(".rtot_tax").each(function () {
                var indTax = $(this).val();

                gtTax = parseFloat(gtTax) + parseFloat(indTax);
            });

            $("[id$=total_tax_amountR]").text(parseFloat(gtTax).toFixed(2));
            $(".rtotal_price").each(function () {
                var indtot = $(this).val();
                gtTotal = parseFloat(gtTotal) + parseFloat(indtot);
            });
            $(".rquty").each(function () {
                var subQty = this.value;
                gtQty = parseFloat(gtQty) + parseFloat(subQty);
            });

            $(".rsubtotal").each(function () {
                var subTot = this.value;
                gtSubTotal = parseFloat(gtSubTotal) + parseFloat(subTot);
            });
            $(".ritem_discount").each(function () {
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
            $("[id$=totalR]").text((gtTotal).toFixed(2));
            $("[id$=ItemCountR]").val(tbody.children().length);
            $("[id$=ItemQtyR]").text((gtQty).toFixed(2));
            $("[id$=SubTotalR]").text((gtSubTotal).toFixed(2));
            $("[id$=ItemDiscR]").text(parseFloat(gtDiscount).toFixed(2));
        }
    }
}
//item unit change
function unitchangeR(selectObject, arg, action) {
    minstockcheckR(arg);
    var index = $('#runit_name_' + arg).prop('selectedIndex');

    if (index == 1) {
        var unitId = parseFloat($('#runit_name_' + arg).val());
        var cfactor = parseFloat($('#rcfactor_' + arg).val());
        var price = parseFloat($("#rbase_rate_" + arg).attr("data-value"));
        var newprice = parseFloat(price / cfactor);
        $(".rprice_item_" + arg).val(newprice.toFixed(2));
    } else {
        var unitId = parseFloat($('#runit_name_' + arg).val());
        //var cfactor = parseFloat($('#cfactor_' + arg).val());
        var price = parseFloat($("#rbase_rate_" + arg).attr("data-value"));
        //var newprice = parseFloat(price * cfactor);
        $(".rprice_item_" + arg).val(price.toFixed(2));
    }

    rowSubTotalR(arg);
    CalculatetblItemListSumR();
    grandtotalcalculationR();
    paidamountcalculationR();
}

//Delete a row of table
function deleteRowR(t) {
    var classname = $(t).closest('tr').attr('class');
    if (classname == 'ritem_') alert("Sorry you can't delete this row.");
    else {
        var r = confirm("Are you sure you want to delete this..?");
        if (r == true) {
            var e = t.parentNode.parentNode;
            e.parentNode.removeChild(e);
        }
    }
    CalculatetblItemListSumR();
    grandtotalcalculationR();
    paidamountcalculationR();
    var i = 1;
    $('#addinvoiceItempopup tr').each(function () {
        $(this).find('td:first').text(i);
        i++;
    });
}

var bcount = 1, btype = '';
blimits = 50;
function addbillsundryrtn(t, action, BsValue, AmountType, BsAmount, BsType, BSName, billsundry) {
    if (bcount == blimits) alert("You have reached the limit of adding " + bcount + " inputs");
    else {
        var data = "";
        var Type = "";
        var Option = "";
        var readonly = "";
        var row = "<tr class='rbs_'>";
        var slno = $('#addbillsundrypopup tr').length + 1;
        tabindex = bcount * 5;
        tab1 = tabindex + 1;
        tab2 = tabindex + 2;
        tab3 = tabindex + 3;
        tab4 = tabindex + 4;
        tab5 = tabindex + 5;
        if (billsundry != null) {
            row = "<tr class='rbs_" + billsundry + "'>";
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
           "<td class='input-group input-group-sm'><select data-name='BillSundry' class='form-control rbsname' data-id='" + bcount + "' placeholder='Bill Sundry Name' id='rbsname'  data-val-required='The bill sundry name field is required' onchange='GetBillSundrydetailsR(this," + bcount + ")'>" + Option + "</select></td>" +
           "<td><input type='number' data-name='BsValue' " + readonly + " value='" + BsValue + "'  class='form-control rbsvalue_" + bcount + "' onchange='bsvaluechangeR(" + bcount + ");' id='rbsvalue_" + bcount + "' data-id='" + bcount + "' id='rbsvalue' /></td>" +
           "<td><input type='text' data-name='' value='" + Type + "' class='form-control rbsamttype_" + bcount + "' id='rbsamttype_" + bcount + "' data-id='" + bcount + "' id='rbsamttype' readonly='readonly'/></td>" +
           "<td><input type='number' data-name='BsAmount' value='" + BsAmount + "' class='form-control rbsamt rbsamt_" + bcount + "' onchange='bsamtchangeR(" + bcount + ");' id='rbsamt_" + bcount + "' data-id='" + bcount + "' id='rbsamt' value='0.00' placeholder='0.00'/><input type='hidden' data-name='AmountType'  value='" + AmountType + "' class='ramttypevalue' name='amttypevalue' id='ramttypevalue_" + bcount + "'/><input type='hidden' value='" + BsType + "' data-name='BsType'  class='rbstype' name='bstype' id='rbstype_" + bcount + "'/></td>" +
           "<td class='text-center'><button style='text-align: right;' class='btn btn-danger' type='button' value='Delete' onclick='deletebsRowR(this)'><i class='fa fa-trash fa-1x'></i></button></td>",
        row += data + "</tr>";
        $('#' + t).append(row);
        searchbsR();
        bcount++;
        setTabIndexR();
    }
}
function searchbsR() {

    var selecteditem = new Array();
    $(".rbsname").each(function () {
        selecteditem.push($(this).val());
    });

    $(".rbsname").select2({
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
function GetBillSundrydetailsR(selectObject, dataid) {
    var SbId = selectObject.value;
    if (SbId != null) {
        if ($(".rbs_" + SbId).length > 0) {
            if (confirm('Are you sure want to Add this Bill Sundry Again?')) {
                bsUpdateR(selectObject, dataid);
            }
        }
        else {
            bsUpdateR(selectObject, dataid);
        }
    }
}
function bsUpdateR(selectObject, dataid) {
    $.ajax({
        url: '/BillSundry/GetBillSundryById',
        type: "GET",
        dataType: "JSON",
        data: { bsID: selectObject.value },
        success: function (result) {
            //additive/subtrative
            $("#rbstype_" + dataid).val(result.BSType);

            //percentage/amt
            $("#ramttypevalue_" + dataid).val(result.AmountType);
            $("#rbsvalue_" + dataid).val(result.DefaultValue);
            var defvalue = $("#rbsvalue_" + dataid).val();

            BindBsAmountR(dataid, defvalue);
            grandtotalcalculationR();


            $(selectObject).closest('tr').attr('class', "rbs_" + result.BillSundryId);
            if ($(".rbs_").length == 0) {
                addbillsundryrtn('addbillsundrypopup', '', '0.00', '', '0.00', '');
            }

        }
    });
}
//percentage cal on billsundry
function calculatePercentageR(dataid) {
    var total = parseFloat($("#total").text());
    total = (total > 0) ? total : 0;
    var value = parseFloat($("#rbsvalue_" + dataid).val());
    var amt = (total * (value / 100));
    $("#rbsamt_" + dataid).val(amt.toFixed(2));
}

function BindBsAmountR(dataid, defvalue) {
    var value = parseFloat($("#rbsvalue_" + dataid).val());
    var bstype = parseFloat($("#rbstype_" + dataid).val());
    var amtype = $("#ramttypevalue_" + dataid).val();
    var total = parseFloat($("#rtotal").text());


    total = (total > 0) ? total : 0;
    if (amtype == 0) {
        $("#rbsvalue_" + dataid).val("").attr('readonly', true);
        $("#rbsamttype_" + dataid).val("");
        $("#rbsamt_" + dataid).val(parseFloat(defvalue).toFixed(2));
        $("#rbsamt_" + dataid).focus();
    } else {
        $("#rbsvalue_" + dataid).focus();
        $("#rbsvalue_" + dataid).val(defvalue);
        $("#rbsamttype_" + dataid).val("%");
        $("#rbsvalue_" + dataid).attr('readonly', false);
        calculatePercentage(dataid);
    }
    grandtotalcalculationR();
}
function grandtotalcalculationR() {
    var gtTotal = parseFloat($("#totalR").text());
    gtTotal = (gtTotal > 0) ? gtTotal : 0;
    $("#addbillsundrypopup tr").each(function () {
        var type = parseFloat($(this).find('.rbstype').val());
        var amt = $(this).find('.rbsamt').val();

        amt = (amt > 0) ? amt : 0;
        if (type == 0) {
            gtTotal = parseFloat(gtTotal) + parseFloat(amt);
        } else if (type == 1) {

            gtTotal = parseFloat(gtTotal) - parseFloat(amt);
        }
    });
    $("#SRGrandTotal").val(parseFloat(gtTotal).toFixed(2));
    //FCCalculationR();
    paidamountcalculationR();
}

//onchange of billsundry value
function bsvaluechangeR(arg) {
    var defvalue = $("#rbsvalue_" + arg).val();
    BindBsAmountR(arg, defvalue);
}
//amt chnage
function bsamtchangeR(arg) {
    var defvalue = parseFloat($("#rbsamt_" + arg).val());
    BindBsAmountR(arg, defvalue);
}

//Delete a row of table
function deletebsRowR(t) {
    var classname = $(t).closest('tr').attr('class');
    if (classname == 'rbs_') alert("Sorry You Can't Delete This Row.");
    else {
        var r = confirm("Are you sure you want to delete this..?");
        if (r == true) {
            var e = t.parentNode.parentNode;
            e.parentNode.removeChild(e);
        }
    }
    grandtotalcalculationR();
    var i = 1;
    $('#addbillsundrypopup tr').each(function () {
        $(this).find('td:first').text(i);
        i++;
    });
}

//print item bill sundry
//itembind
function bindItemR(e, dvitem) {
    var total = parseFloat(0);
    var str = "";
    var itemcode = "";
    var count = 1;
    var qty = 0;
    var wgt = 0;
    var cbm = 0;
    var Layout = (typeof e.layout == 'undefined') ? "Default" : e.layout.Name;
    var UnitName = "";
    $("#PoNo").hide();
    function ItemsBindR(ritem, rtype, bcount) {
        var Row = "";
        var unit = (ritem.ItemUnit != null) ? ritem.ItemUnit : "";
        var PartNo = (ritem.PartNumber != null && ritem.PartNumber != "") ? ritem.PartNumber : "";
        var itemnote = "";
        if (ritem.ItemNote != "" && ritem.ItemNote != "-:{Bundle_Item}") {
            itemnote = "<br /><small>" + ritem.ItemNote + "</small>";
        }
        var dvField1 = "";
        var dvField2 = "";
        var trcount = rtype == "bundle" ? bcount : count;
        if (dvitem != "active" && rtype != "bundle") {
            dvField1 += '<td class="text-right"><b>' + parseFloat(ritem.ItemUnitPrice).toFixed(2) + '</b></td>';
            dvField1 += '<td class="text-right"><b>' + parseFloat(ritem.ItemSubTotal).toFixed(2) + '</b></td>';
            dvField2 += '<td class="text-right">' + parseFloat(ritem.ItemTaxAmount).toFixed(2) + '</td>';
            dvField2 += '<td class="text-right">' + parseFloat(ritem.ItemTotalAmount).toFixed(2) + '</td>';
        } else if (dvitem != "active" && rtype == "bundle") {
            dvField1 += '<td></td><td></td>';
            dvField2 += '<td></td><td></td>';
        }
        Row += '<tr class="border-top">';
        Row += '<td>' + trcount + '</td>';
        if (ritem.PNoStatus == 0) {
            $("#PoNo").show();
            Row += '<td>' + PartNo + '</td>';
        }
        // Default Invoice Structure
        if (Layout == "Default") {
            if (e.summary.chkCode == 0) {
                itemcode = ritem.ItemCode + " - ";
            }
            Row += '<td>' + itemcode + ritem.ItemName + itemnote + '</td>';
            Row += '<td>' + unit + '</td>';
            Row += '<td>' + ritem.ItemQuantity + '</td>';
            Row += dvField1 + dvField2;
        }
        else if (Layout == "Jewellery") {
            Row += '<td>' + ritem.ItemCode + '</td>';
            Row += '<td>' + ritem.ItemName + itemnote + '</td>';
            Row += '<td>' + ritem.ItemQuantity + '</td>';
            Row += '<td>' + ritem.ItemQuantity + '</td>';
            Row += dvField1;
        }
        else if (Layout == "Scaffold") {
            var CBM = (ritem.CBM != null && ritem.CBM != "") ? (parseFloat(ritem.CBM) * parseFloat(ritem.ItemQuantity)).toFixed(3) : "";
            var Weight = (ritem.Weight != null && ritem.Weight != "") ? (parseFloat(ritem.Weight) * parseFloat(ritem.ItemQuantity)).toFixed(3) : "";
            var img = "";
            wgt = parseFloat(wgt) + parseFloat(Weight || 0);
            cbm = parseFloat(cbm) + parseFloat(CBM || 0);if (ritem.img != null && ritem.img.length > 0) {
                $.each(ritem.img, function (j, imgs) {
                    var im = "/uploads/itemimages/" + ritem.Id + "/thumb_" + imgs.FileName;
                    img = "<img width='68' height='46' src='/uploads/itemimages/" + ritem.Id + "/thumb_" + imgs.FileName + "'/>";
                    // img = "<div style='width:50px;height:50px;background:url(" + im + ");background-size: cover;'></div>";
                });
                if (rtype != "bundle") {
                    Row += '<td><b>' + ritem.ItemName + "</b>" + itemnote + '</td>';
                } else {
                    Row += '<td>' + ritem.ItemName + itemnote + '</td>';
                }

                Row += '<td style="width:70px; padding:1px;">' + img + '</td>';
            }
            else {
                if (rtype != "bundle") {
                    Row += '<td colspan="2"><b>' + ritem.ItemName + "</b>" + itemnote + '</td>';
                } else {
                    Row += '<td colspan="2">' + ritem.ItemName + itemnote + '</td>';
                }
            }
            if (rtype != "bundle") {
                Row += '<td><b>' + Weight + '</b></td>';
                Row += '<td><b>' + CBM + '</b></td>';
                Row += '<td><b>' + ritem.ItemQuantity + ' ' + unit + '</b></td>';
            } else {
                Row += '<td>' + Weight + '</td>';
                Row += '<td>' + CBM + '</td>';
                Row += '<td>' + ritem.ItemQuantity + ' ' + unit + '</td>';

            }
            Row += dvField1;

        }
        Row += '</tr>';
        return Row;
    }
    $.each(e.item, function (i, item) {
        qty += item.ItemQuantity;

        var subtot = parseFloat(item.ItemTotalAmount.toFixed(2));
        total += subtot;

        str += ItemsBindR(item);
        count++;
        // bundle items
        if (item.bundle != null && item.bundle.length > 0) {
            $.each(item.bundle, function (j, itemss) {
                var bcount = j + 1
                str += ItemsBindR(itemss, "bundle", bcount);
            });
        }
    });
    if (Layout == "Jewellery") {
        str += '<tr id="jwltotal" class="border-top"><td colspan="2"><b>(' + (count - 1) + ' items)</b></td><td class="text-center"><b> Total الجمالى</b></td>';
        str += '<td><b>' + parseFloat(qty).toFixed(2) + '</b></td><td><b>' + parseFloat(qty).toFixed(2) + '</b></td><td></td><td class="text-right"><b>' + parseFloat(total).toFixed(2) + '</b></td></tr>';
    }
    if (Layout == "Scaffold") {
        var weihtv = (parseFloat(wgt) != 0) ? parseFloat(wgt).toFixed(3) + " Kg" : "";
        var cbmv = (parseFloat(cbm) != 0) ? parseFloat(cbm).toFixed(3) : "";
        str += "<tr class='border-top'><td colspan='3' class='text-right'><b>TOTAL</b></td><td class='text-center'><b>" + weihtv + "</b></td><td class='text-center'><b>" + cbmv + "</b></td><td></td><td></td><td></td></tr>";
    }
    return str;
}


// bind bill sundry
function bindSundryR(e) {
    var str = "";
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

        str += "<tr class='border-top'>";
        str += '<td>' + billsundry.BillSundry + '</td>';
        str += '<td class="text-right">' + parseFloat(billsundry.BsAmount).toFixed(2) + '</td>';
        str += '</tr>';
    });
    return str;
}

//change cofactor
function coFactorChangeR() {
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


function setTabIndexR() {
    var j = 1;
    $('body').find('input,textarea,select,button, .select2-container .select2-selection__rendered').not(".select2-hidden-accessible").not(":hidden").each(function (i) {
        if (!$(this).hasClass("select2-hidden-accessible") && !$(this).is(":hidden")) {
            $(this).attr('tabindex', j);
            j++;
        }
        if ($(this).closest("tr").hasClass("ritem_") && !$(this).hasClass("select2-selection__rendered")) {
            $(this).attr('tabindex', -1);
        }
    });
}

function SaleReturnPopUp(fnval, Mode, SaleEntryID)
{
      $.fn.modal.Constructor.prototype.enforceFocus = function () { };
        $('#modal-returninsale').on('shown.bs.modal', function (e) {
            $("#ddlRMC").select2({
                placeholder: 'Search Material Center by Name or Code',
                minimumInputLength: 0,
                ajax: {
                    url: "/MC/SearchMCUser",
                    dataType: 'json',
                    delay: 50,
                    data: function (params) {
                        return {
                            q: params.term,
                            page: params.page,
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
        $('#ddlRMC').select2({
            dropdownParent: $('#modal-returninsale')
        });


        $.ajax({
            url: '/CreditSaleReturn/GetMaxReturnId',
            type: "GET",
            dataType: "JSON",
            data: { SalesEntryId: SaleEntryID, SaveMode: Mode},
            success: function (result) {
                $("#SReturnId").val(result);
            }
        });

    var data = $('#ddlCustomer').select2('data');
    $("#CustomerName").val(data[0].text);


    var tbody1 = $("#normalinvoicez tbody");
    if (tbody1.children().length == 0) {
        addrowrtn('addinvoiceItempopup', 'sales', "", "0.00", "0.00", "0");
    }

    var tbody2 = $("#billsundryz tbody");
    if (tbody2.children().length == 0) {
        addbillsundryrtn('addbillsundrypopup', 'sales', '0.00', '', '0.00', '');
    }

    //$('#addinvoiceItempopup').html('');
    //$('#addbillsundrypopup').html('');
    $("#modal-returninsale").modal({ show: true, backdrop: "static" });
}