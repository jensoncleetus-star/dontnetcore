var count = 1, type = '';
limits = 500;
//Add Row 
var pcount = 2;
function addrowused(t, action, ItemUnit, ItemTax, ItemTotalAmount, ItemQuantity, Item, ItemCode, ItemName, ItemUnitPrice, ItemSubTotal, ItemWithCode, ItemTaxAmount, ItemDiscount, ItemNote, itemdata, ConFactor, ItemCategory, ItemBrand, PPRICE, MRP, SellPrice, BPrice, tag1, tag2, tag3, tag4, tag5, prefix, division, supplier) {

    if (count == limits) alert("You have reached the limit of adding " + count + " inputs");
    else {
        var Option = "";
        var optionunit = "";
        var required = "";
        var slno = $('#addinvoiceusedpopup tr').length + 1;
        var a = "uitem_name" + count,
            tabindex = count * 5;
        var row = "<tr class='uitem_' id='uitem_" + count + "'>";
        var data = "";
        var price = 0;
        var baseprice = 0;
        var mrp = 0;
        var htdata = "";
        var itemnote = "";
        var notbtn = "";
        var ConFactor = "";
        var divid = "uitem_name_" + Item;
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
            row = "<tr class='uitem_" + Item + "' id='uitem_" + count + "'>";
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
        itemnote = '<div id="umodal-item-' + count + '" class="modal fade" role="dialog" aria-hidden="true"><div class="modal-dialog"><div class="modal-content">' +
            '<div class="form-group"><textarea name="uitemnote" cols="40" rows="10" class="form-control uitemnote" id="uitemnote-' + count + '" maxlength="1000">' + inote + '</textarea></div>' +
            '<div class="form-group"><button class="btn btn-info" type="button" data-dismiss="modal">Save</button></div>' +
            '</div></div></div>';
        notbtn = "<button type='button' class='ritnote btn btn-default btn-flat' data-toggle='modal' data-target='#umodal-item-" + count + "'><i class='fa fa-1x fa-file-text-o'></i></button>";

        if (itemdata) {
            ConFactor = itemdata.ConFactor != null ? itemdata.ConFactor : 1;
            if (type == "purchase")
                price = itemdata.PurchasePrice;
            else
                price = itemdata.PurchasePrice;
            baseprice = itemdata.PurchasePrice;
            ConFactor = ConFactor;
            mrp = itemdata.MRP;
            if (type == "sales") {
                htdata = "<div class='uminstock_" + count + "'";
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


        ItemDiscount = ItemDiscount != null ? ItemDiscount : 0;
        ItemCategory = ItemCategory != null ? ItemCategory : 0;
        ItemBrand = ItemBrand != null ? ItemBrand : 0;
        PPRICE = PPRICE != null ? PPRICE : 0;
        MRP = MRP != null ? MRP : 0;
        SellPrice = SellPrice != null ? SellPrice : 0;
        BPrice = BPrice != null ? BPrice : 0;
        data = "<td class='text-center' id=" + divid + "> " + slno + " </td>" +
            "<td class='input-group input-group-sm'><select class='form-control uitem_name' " + required + " data-id='" + count + "' placeholder='Item Name' id='uitem_name_" + count + "'  data-msg-required='The Item field is required' onchange='GetItemdetailsU(this," + count + ",\"" + type + "\")'>" + Option + "</select> " + itemaddbtn + "</td>" +
            "<td style='width:100px;'><select class='form-control uunits uunit_name_" + count + "' id='uunit_name_" + count + "' data-id='" + count + "' id='uunit_name' onchange='unitchangeU(this," + count + ",\"" + type + "\"); '></select></td>" +
            "<td> <input type='number' name='product_quantity[]' data-msg-min ='The Item Quantity must be Greater than Zero' onchange='quantity_changeU(" + count + ");' id='utotal_qntt_" + count + "' value='" + ItemQuantity + "'  class='utotal_qntt_" + count + " form-control text-right uquty' placeholder='0' value='0' min='.01' tabindex='" + tab2 + "'/></td>" +
            "<td><input type='number' name='product_rate[]' " + required + " data-msg-required='The Item Rate field is required' readonly onchange='rate_changeU(" + count + ",\"" + type + "\");' id='uprice_item_" + count + "' value='" + ItemSubTotal + "' class='uprice_item_" + count + " form-control text-right utotrate' placeholder='0.00' min='0' tabindex='" + tab3 + "'/><input type='hidden' data-value='" + price + "' value='" + baseprice + "' name='base_rate' id='ubase_rate_" + count + "'> </td> " +
            "<td><input type='number' name='sub_total[]' id='usub_total_" + count + "' class='usub_total_" + count + " form-control text-right usubtotal' value='" + ItemSubTotal + "'   placeholder='0.00' min='0' tabindex='" + tab3 + "' readonly='readonly'/></td>" +
            "<input type='hidden' name='item_discount[]' id='uitem_discount" + count + "' onchange='itemdiscount_changeU(" + count + ");' class='uitem_discount" + count + " form-control text-right uitem_discount' value='" + ItemDiscount + "' value='0.00' placeholder='0.00' tabindex='" + tab3 + "'/>" +
            "<td><input type='text' id='utax_" + count + "' class='form-control text-right utax utax_" + count + "' tabindex='" + tab4 + "' readonly='readonly' /><input type='hidden' class='uitem_amount' name='item_amount' id='uitem_amount_" + count + "'/><input type='hidden' class='utot_tax' name='tot_tax' id='utot_tax_" + count + "'/><input type='hidden'  class='utax_percentage' value='" + ItemTax + "' name='tax_percentage' id='utax_percentage_" + count + "'/></td> " +
            "<td class='text-right'><input class='utotal_price rtotal_price_" + count + " form-control text-right' type='text' name='total_price[]' value='" + ItemTotalAmount + "' id='utotal_price_" + count + "' value='0.00' readonly='readonly' style='display:none'/><input type='hidden' class='ucfactor' value='" + ConFactor + "' name='cfactor' id='ucfactor_" + count + "'/> " +
            "<input type='hidden' class='uselitem_name_" + count + "' value='" + ItemName + "' name='selitem_name' id='uselitem_name_" + count + "'/> " +
            "<input type='hidden' class='uselitem_code_" + count + "' value='" + ItemCode + "' name='selitem_code' id='uselitem_code_" + count + "'/> " +
            "<input type='hidden' class='uselitem_category_" + count + "' value='" + ItemCategory + "' name='selitem_category' id='uselitem_category_" + count + "'/> " +
            "<input type='hidden' class='uselitem_brand_" + count + "' value='" + ItemBrand + "' name='selitem_brand' id='uselitem_brand_" + count + "'/> " +
            "<input type='hidden' class='uselitem_pprice_" + count + "' value='" + PPRICE + "' name='selitem_pprice' id='uselitem_pprice_" + count + "'/> " +
            "<input type='hidden' class='uselitem_pmrp_" + count + "' value='" + MRP + "' name='selitem_pmrp' id='uselitem_pmrp_" + count + "'/> " +
            "<input type='hidden' class='uselitem_psprice_" + count + "' value='" + SellPrice + "' name='selitem_psprice' id='uselitem_psprice_" + count + "'/> " +
            "<input type='hidden' class='uselitem_pbprice_" + count + "' value='" + BPrice + "' name='selitem_pbprice' id='uselitem_pbprice_" + count + "'/> " +
            "<input type='hidden' class='utagline1_" + count + "' value='" + tag1 + "'  name='tagline1' id='utagline1_" + count + "'/> " +
            "<input type='hidden' class='utagline2_" + count + "' value='" + tag2 + "'  name='tagline2' id='utagline2_" + count + "'/> " +
            "<input type='hidden' class='utagline3_" + count + "' value='" + tag3 + "'  name='tagline3' id='utagline3_" + count + "'/> " +
            "<input type='hidden' class='utagline4_" + count + "' value='" + tag4 + "' name='tagline4' id='utagline4_" + count + "'/> " +
            "<input type='hidden' class='utagline5_" + count + "' value='" + tag5 + "' name='tagline5' id='utagline5_" + count + "'/> " +
            "<input type='hidden' class='uprefix_" + count + "' value='" + prefix + "' name='prefix' id='uprefix_" + count + "'/> " +
            "<input type='hidden' class='udivision_" + count + "' value='" + division + "' name='division' id='udivision_" + count + "'/> " +
            "<input type='hidden' class='usupplier_" + count + "' value='" + supplier + "' name='supplier' id='usupplier_" + count + "'/> " +
            "</td>" +
            "<td class='text-center'><button tabindex='" + tab5 + "' style='text-align: right;' class='btn btn-danger' type='button' value='Delete' onclick='deleteRowU(this)'><i class='fa fa-trash fa-1x'></i></button>" + itemnote + htdata + "</td>";
        row += data + "</tr>";
        $('#' + t).append(row);
        // $('#item_ .item_name').focus();
        searchItemU();
        if (itemdata) {
            rate_changeU(count, type, 'foredit');
            createUnitListU(itemdata, count, action);
        }
        else
            rate_changeU(count, type);

        if (itemdata) {

            var BatchEnable = $("#batchcheck").val();
            var VoucherType = $("#VoucherType").val();

            $("#batch-" + count).remove();
            if (BatchEnable == "active" && itemdata.KeepStock == true && itemdata.slreq == true) {

                if (VoucherType == "Purchase" || VoucherType == "SalesReturn" || VoucherType == "Sales" || VoucherType == "PurchaseReturn") {

                    // create modal
                    SBPurModals(itemdata, count, 'edit');
                    // add data to modal
                    $.each(itemdata.batch, function (i, bst) {
                        //if (count == bst.Order || (VoucherType == "SalesReturn" && bst.origin == "Sales") || (VoucherType == "PurchaseReturn" && bst.origin == "Purchase")) {
                        if (VoucherType == "Purchase" || VoucherType == "SalesReturn") {
                            if (bst.origin == "Sales") {
                                bst.StockIn = bst.StockOut;
                                bst.StockOut = 0;
                            }
                            addSBPURRows(count, Item, bst);
                        }
                        if (VoucherType == "Sales" || VoucherType == "PurchaseReturn") {
                            if (bst.origin == "Purchase") {
                                bst.StockOut = bst.StockIn;
                                bst.StockIn = 0;
                            }
                            addSBSalRows(count, Item, bst);
                        }
                        // }
                    });
                    if (VoucherType == "Purchase" || VoucherType == "SalesRetpdateurn") {
                        addSBPURRows(count, Item);
                    }
                    if (VoucherType == "Sales" || VoucherType == "PurchaseReturn") {
                        addSBSalRows(count, Item);
                    }
                }
            }
        }
        count++;
        setTabIndexU();
    }
}



//item details
function GetItemdetailsU(selectObject, dataid, action) {
    if (selectObject.value) {
        var ItemId = selectObject.value;
        if (ItemId != null) {
            if ($(".uitem_" + ItemId).length > 0) {
                if ($(".uitem_" + ItemId).length < 40) {
                    if (confirm('Are you sure want to Add this item Again?')) {
                        itemUpdateU(selectObject, dataid, action);
                    }
                    else {
                        $(selectObject).val(null).trigger('change');
                    }
                }
                else {
                    alert("You Cannot Add same Item More than 40 Times");
                    $(selectObject).val(null).trigger('change');
                }
            }
            else {
                itemUpdateU(selectObject, dataid, action);
            }
        }
    }
}
// update item details
function itemUpdateU(selectObject, dataid, action) {
    var mc = $("#ddlMC").val();
    if (action == "sales" || action == "quot") {
        var ROnlyRate = $("#UOnlyRate").val();
        if (ROnlyRate == "active") {
            $("#uprice_item_" + dataid).attr('readonly', true);
        }
    }
    var Stype = $('#SaleType').val();
    var Hire = $('#ddlHType').val();


    var newUrl;
    if (mc != null && mc > 0 && Stype == 1) {
        newUrl = '/Item/GetItemMC';
    }
    else {
        newUrl = '/Item/GetItemMC';
    }



    $.ajax({
        url: newUrl,
        type: "GET",
        dataType: "JSON",
        data: { itemID: selectObject.value, mc: mc, Saltype: Stype, HireType: Hire },
        success: function (result) {
            createUnitListU(result, dataid, action);
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
                $("#uprice_item_" + dataid).val(result.PurchasePrice);
                $("#uitem_amount_" + dataid).val(result.SellingPrice);
                $("#ubase_rate_" + dataid).attr("data-value", result.PurchasePrice);
                //}
            }
            if (action == "purchase") {
                $(".uprice_item_" + dataid).val(result.PurchasePrice);
                $("#uitem_amount_" + dataid).val(result.SellingPrice);
                $("#ubase_rate_" + dataid).attr("data-value", result.PurchasePrice);
            }
            $("#utotal_qntt_" + dataid).val(1);
            var SalesType = $("#SalesType").val() || "";
            var PurchaseType = $("#PurchaseType").val() || "";
            var TaxP = result.Tax;
            if (PurchaseType == 2 || SalesType == 2) {
                TaxP = 0;
            }
            $("#utax_percentage_" + dataid).val(TaxP);
            $("#ubase_rate_" + dataid).val(result.BasePrice);
            $("#ucfactor_" + dataid).val(result.ConFactor);
            rowSubTotalU(dataid);
            CalculatetblItemListSumU();
            grandtotalcalculationU();
            paidamountcalculationU();
            bspcalculate();
            if (action != "sales" || (action == "sales" && result.KeepStock != true) || (action == "sales" && result.KeepStock == true && result.total > 0)) {
                // append item unit list 

                $(selectObject).closest('tr').attr('class', "uitem_" + result.ItemID);
                if (action == "sales") {
                    minstockupdateU(result, dataid);
                }
                if ($(".uitem_").length == 0) {
                    addrowused('addinvoiceusedpopup', '', '', '0.00', '0.00', '0');
                }
                $('.uunit_name_' + dataid).focus();
            }
            else if ((result.KeepStock == true && result.CheckStock == 0 && result.total <= 0)) {

                var res = confirm("Are you Sure Want To Add Items In Less Stock ?");
                if (res == true) {
                    $(selectObject).closest('tr').attr('class', "uitem_" + result.ItemID);
                    if (action == "sales") {
                        minstockupdateU(result, dataid);
                    }
                    if ($(".uitem_").length == 0) {
                        addrowused('addinvoiceusedpopup', '', '', '0.00', '0.00', '0');
                    }
                    $('.uunit_name_' + dataid).focus();
                }
                else {
                    $("#uitem_name_" + dataid).val(null).trigger("change");
                    $("#uunit_name_" + dataid).val(null).trigger("change");
                    $("#utotal_qntt_" + dataid).val(null).trigger("change");
                    $("#uprice_item_" + dataid).val(null).trigger("change");
                }
            }
            else {
                alert("This Item is Out of Stock!!!");
                var classname = $($("#utotal_qntt_" + dataid)).closest('tr').attr('class');
                if (classname != 'uitem_') {
                    $("." + classname + " .btn-danger").click();
                }
                else {
                    $("#uitem_name_" + dataid).val(null).trigger("change");
                    $("#runit_name_" + dataid).val(null).trigger("change");
                    $("#rtotal_qntt_" + dataid).val(null).trigger("change");
                    $("#rprice_item_" + dataid).val(null).trigger("change");
                }
            }
            // batch stock updates
            var BatchEnable = $("#batchcheck").val();
            var VoucherType = $("#VoucherType").val();
            $("#batch-" + dataid).remove();
            if (BatchEnable == "active" && result.KeepStock == true && result.slreq == true) {
                if (VoucherType == "Purchase" || VoucherType == "SalesReturn") {
                    $("#total_qntt_" + dataid).val(0);
                    SBPurModals(result, dataid);
                    addSBPURRows(dataid, result.ItemID);
                }
                else if (VoucherType == "Sales" || VoucherType == "PurchaseReturn") {

                    $("#utotal_qntt_" + dataid).val(0);

                    SBPurModals(result, dataid);

                    addSBSalRows(dataid, result.ItemID);
                }
            }
        }
    });
}
function minstockupdateU(result, dataid) {
    var htdata = "<div class='uminstock_" + dataid + "'";
    if (result.KeepStock == true) {
        totalstock = result.total;
        minstock = result.MinStock * result.ConFactor;

        htdata += " data-keeps='yes' data-min='" + minstock + "' data-confactor='" + result.ConFactor + "' data-stock='" + totalstock + "'>";
    }
    else {
        htdata += " data-keeps='no' >";
    }
    if ($(".uminstock_" + dataid).length) {
        $(".uminstock_" + dataid).remove();
    }
    $('#uitem_' + dataid).append(htdata);
}
function minstockcheckU(arg) {
    var keepstock = $(".uminstock_" + arg).attr('data-keeps');
    if (keepstock == "yes") {
        var index = $('#uunit_name_' + arg).prop('selectedIndex');
        var unitname = $('#uunit_name_' + arg).find('option:selected').text();
        var minstock = parseFloat($(".uminstock_" + arg).attr('data-min'));
        var confactor = parseFloat($(".uminstock_" + arg).attr('data-confactor'));
        var stock = parseFloat($(".uminstock_" + arg).attr('data-stock'));
        var quantity = parseFloat($(".utotal_qntt_" + arg).val());

        var qty = 0;
        var classn = $("#uitem_" + arg).attr('class');

        $("." + classn).each(function () {

            var rowid = $(this).attr('id');
            var arr = rowid.split('_');
            var arg1 = arr[1];
            var index1 = $("#" + rowid + " .uunits").prop('selectedIndex');
            var curent = $("#" + rowid + " .uquty").val();
            var confactor1 = parseFloat($("#" + rowid + "  .uminstock_" + arg1).attr('data-confactor'));
            if (index == 0) {
                qty += (parseInt(curent) * parseInt(confactor1));
            }
            else {
                qty += parseInt(curent);
            }
        });
        if (index == 0) {
            stock = stock - (parseInt(qty) - parseInt(quantity));
            minstock = minstock / confactor;
            stock = stock / confactor;
            var tostock = stock - quantity;
            var totstock = tostock / confactor;

            //var totstock = stock - qty;
            if (totstock <= minstock && totstock >= 0) {
                alert("Stock Exceeds Minimum Stock");
            }
            else if (quantity >= stock && stock <= 0) {
                $(".utotal_qntt_" + arg).val(quantity);
                stock = stock - (qty - quantity);
            }
            else if (totstock < 0) {
                stock = stock.toFixed(2);
                alert("This Item Is Going To Out of Stock!!! Only " + stock + " " + unitname + "Items Are Available In Stock..");
                $(".utotal_qntt_" + arg).val(parseInt(stock));
            }

        } else {
            stock = stock - (qty - quantity);
            var totstock = stock - quantity;
            if (totstock <= minstock && totstock >= 0) {
                // alert("Stock Exceeds Minimum Stock");
            }
            if (totstock < 0) {
                // alert("This Item Is Going To Out of Stock!!! Only " + stock + " " + unitname + " Items Are Available In Stock..");
                // $(".utotal_qntt_" + arg).val(stock);
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
function createUnitListU(result, dataid, action) {
    // clear previous content
    if (action == "sales" || action == "quot" || action == "foredit") {
        var ROnlyRate = $("#UOnlyRate").val();
        if (ROnlyRate == "active") {
            $("#Uprice_item_" + dataid).attr('readonly', true);
        }
    }
    $('#Uunit_name_' + dataid).empty();
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

            $('#uunit_name_' + dataid).append(newOption);
            $('#uunit_name_' + dataid).append(newOption1);
        }
        else {
            newOption.val(result.ItemUnitID).html(result.PriUnit);
            $('#uunit_name_' + dataid).append(newOption);
        }
    }
    else {

    }
}
// search item
function searchItemU() {
    var selecteditem = new Array();

    $(".uitem_name").each(function () {
        selecteditem.push($(this).val());
    });
    var mc = $("#ddlMC").val();
    if (mc != null && mc > 0) {
        $(".uitem_name").select2({
            placeholder: 'Search Item by Code',
            minimumInputLength: 0,
            ajax: {

                url: "/Item/SearchdetailsMCSP",
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
        $(".uitem_name").select2({
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
    }
    rate_changeU(count);
}

function repoFormatResultU(repo) {
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

function repoFormatSelectionU(repo) {
    return repo.text;
}

// modal generation for Purchase Batch Stock
function SBPurModals(result, dataid, type) {

    var BStUnit = "<div class='BStUnit' data-confactor='" + result.ConFactor + "' data-ItemUnitID='" + result.ItemUnitID + "'  data-PriUnit='" + result.PriUnit + "'" +
        " data-SubUnitID='" + result.SubUnitId + "'  data-SubUnit='" + result.SubUnit + "' >" +
        "</div>";
    var Modal = "<div id='batch-" + dataid + "' class='modal fade batch-" + result.ItemID + "' role='dialog' aria-hidden='true'><div class='modal-dialog modal-lg'><div class='modal-content'>" +
        "<div class='modal-header bg-aqua'><button type='button' class='close' onclick='btsclose(" + dataid + ")' style='font-size:30px;color:red;'>&times;</button><h4>" + result.ItemName + " -<span id='ubts_tqty_" + dataid + "'>0</span> <span id='bts_Unit_" + dataid + "'>" + result.PriUnit + "</span> </h3></div>" +
        "<div class='modal-body'>" +
        "<table class='table table-bordered table-hover ubatchtbl' id='ubatchtbl-" + dataid + "'><thead><tr>" +
        "<th class='text-center'>S/N</th><th class='text-center'>Batch No</th><th class='text-center'>MFG</th><th class='text-center'>EXP</th>" +
        "<th class='text-right'>Qty</th><th>Action</th>" +
        "</tr></thead><tbody></tbody><tfoot><tr><th></th><th></th><th></th><th>Total</th><th class='bstotqty text-right'></th><th></th></tr></tfoot></table>" +
        "<div class='form-actions no-color'><input type='button' value='Update' onclick='btsSubmits(" + dataid + ")'  class='btn btn-success col-sm-offset-5'/> " +
        BStUnit + "</div></div></div></div></div>";

    $("#batchStocks").append(Modal);


}
// popup batch stock purchase modal
function PopupBatchStocks(arg, ItemId) {

    var BID = "#batch-" + arg;
    var bstlength = $(BID).length;

    if (bstlength != 0) {
        if (!$(BID).is(':visible')) {
            $(BID).modal({
                backdrop: 'static',
                keyboard: false
            });
        }
    }

}
// add batch stock row in purchase
var sbcount = 1, btype = '';
sblimits = 500;
function addSBPURRows(arg, ItemId, BtData) {

    if (sbcount == sblimits) alert("You have reached the limit of adding " + sbcount + " inputs In Batch Stock");
    else {

        var classn = $("#item_" + arg).attr('class');
        var fields = classn.split('_');
        var border = $("#item_name_" + fields[1]).text();
        var slno = $('#ubatchtbl-' + arg + ' tbody tr').length + 1;
        var BID = "#batch-" + arg;
        var data = "";
        var cfactor = $(BID + " .BStUnit").attr('data-confactor');
        var punit = $(BID + " .BStUnit").attr('data-ItemUnitID');
        var sunit = $(BID + " .BStUnit").attr('data-SubUnitID');
        var punits = $(BID + " .BStUnit").attr('data-PriUnit');
        var sunits = $(BID + " .BStUnit").attr('data-SubUnit');
        var sbunit = parseFloat($('#unit_name_' + arg).val());
        var gtTot = 0;
        var BStockIn = 0;
        var BEXP = "";
        var BMFG = "";
        var BBatchno = "";
        if (BtData) {
            var acSt = parseFloat(BtData.StockIn) / parseFloat(cfactor);
            BStockIn = (sbunit != punit) ? BtData.StockIn : acSt;
            var BEXP = BtData.EXPd != null ? convertToDate(BtData.EXPd) : "";
            var BMFG = BtData.MFGd != null ? convertToDate(BtData.MFGd) : "";
            var BBatchno = BtData.BatchNo;

        }
        $("#item_" + arg + " .utotrate").each(function () {
            var rate = $(this).val();
            rate = rate || 0;
            gtTot = parseFloat(gtTot) + parseFloat(rate);
        });
        var sbtcost = gtTot;
        var row = "<tr class='Bst_" + slno + "'>";
        data = "<td class='text-center'>" + slno + "</td>" +
            "<td><input type='text' data-name='BatchNo' data-count='" + sbcount + "' class='bts_batchno_" + sbcount + " bts_batchnos form-control' onchange='btsqty_changez(this," + arg + ",\"" + ItemId + "\");' required='required' value='" + BBatchno + "' /></td>" +
            "<td class='date'><input type='text' data-name='MFG' class='bts_mfgdate_" + sbcount + " form-control bts_mfgdate datepicker' value='" + BMFG + "'/></td>" +
            "<td class='date'><input type='text' data-name='EXP' class='bts_expdate_" + sbcount + " form-control bts_expdate datepicker' value='" + BEXP + "'/></td>" +
            "<td><input type='number' data-name='StockIn' data-count='" + sbcount + "' data-msg-min ='The Item Quantity must be Greater than Zero' onchange='btsqty_changez(this," + arg + ",\"" + ItemId + "\");' class='bts_qty_" + sbcount + " bts_qntt form-control text-right' placeholder='0' value='" + BStockIn + "' min='.01' required='required' /></td>" +
            "<td class='text-center'><button data-count='" + sbcount + "' class='btn btn-danger' type='button' value='Delete'  onclick='deleteBtsRows(this,\"" + arg + "\")'><i class='fa fa-trash fa-1x'></i></button>" +
            "<input type='hidden' data-name='Item' class='bts_item' value='" + ItemId + "'/>" +
            "<input type='hidden' data-name='cfactor' class='bts_cfactor' value='" + cfactor + "'/>" +
            "<input type='hidden' data-name='Priunit' class='bts_punit' value='" + punit + "'/>" +
            "<input type='hidden' data-name='Secunit' class='bts_sunit' value='" + sunit + "'/>" +
            "<input type='hidden' data-name='Cost' class='bts_cost'  value='" + sbtcost + "'/>" +
            "<input type='hidden' data-name='Unit' class='bts_units' id='bts_unit_name_" + sbcount + "' value='" + sbunit + "'>" +
            "<input type='hidden' data-name='Order' class='bts_order'  value='" + border + "'/>" +
            "</td>";
        row += data + "</tr>";
        $('#ubatchtbl-' + arg + ' tbody').append(row);
        sbcount++;
        totalbtsqty(arg);
        $('.date').datepicker({
            format: 'dd-mm-yyyy',
            autoclose: true,
            allowInputToggle: true,
        });
    }
}
//add for sale
function addSBSalRows(arg, ItemId, BtData) {
    if (sbcount == sblimits) alert("You have reached the limit of adding " + sbcount + " inputs In Batch Stock");
    else {
        var classn = $("#uitem_" + arg).attr('class');
        var fields = classn.split('_');

        var border = $("#uitem_name_" + fields[1]).text();
        var slno = $('#ubatchtbl-' + arg + ' tbody tr').length + 1;
        var BID = "#batch-" + arg;
        var data = "";
        var cfactor = $(BID + " .BStUnit").attr('data-confactor');
        var punit = $(BID + " .BStUnit").attr('data-ItemUnitID');
        var sunit = $(BID + " .BStUnit").attr('data-SubUnitID');
        var punits = $(BID + " .BStUnit").attr('data-PriUnit');
        var sunits = $(BID + " .BStUnit").attr('data-SubUnit');
        var sbunit = parseFloat($('#uunit_name_' + arg).val());
        var VoucherType = $("#VoucherType").val();
        var gtTot = 0;
        var BStockOut = 0;
        var BEXP = "";
        var BMFG = "";
        var BBatchno = "";
        var BOption = "";
        var stmax = "";
        if (BtData) {
            BStockOut = (sbunit != punit) ? BtData.StockOut : (parseFloat(BtData.StockOut) / parseFloat(cfactor));

            if (VoucherType == "Sales") {
                //stmax = BStockOut;
            }
            var BEXP = BtData.EXPd != null ? convertToDate(BtData.EXPd) : "";
            var BMFG = BtData.MFGd != null ? convertToDate(BtData.MFGd) : "";
            BOption = "<option value='" + BtData.BatchNo + "'>" + BtData.BatchNo + "</option>";
        }
        $("#item_" + arg + " .totrate").each(function () {
            var rate = $(this).val();
            rate = rate || 0;
            gtTot = parseFloat(gtTot) + parseFloat(rate);
        });
        var sbtcost = gtTot;
        var row = "<tr class='Bst_" + slno + "'>";
        data = "<td class='text-center'>" + slno + "</td>" +
            "<td><select data-name='BatchNo' data-item='" + ItemId + "' data-count='" + sbcount + "' class='bts_batchno_" + sbcount + " bts_batchno form-control' placeholder='BatchNo' onchange='btsBtch_change(this," + arg + ",\"" + ItemId + "\");' required='required'>" + BOption + "</select> </td>" +
            "<td class='date'><input type='text' readonly data-name='MFG' class='bts_mfgdate_" + sbcount + " form-control bts_mfgdate datepicker' value='" + BMFG + "'/></td>" +
            "<td class='date'><input type='text' readonly data-name='EXP' class='bts_expdate_" + sbcount + " form-control bts_expdate datepicker' value='" + BEXP + "'/></td>" +
            "<td><input type='number' data-name='StockOut' data-count='" + sbcount + "' data-msg-min ='The Item Quantity must be Greater than Zero' onchange='btsqty_changez(this," + arg + ",\"" + ItemId + "\");' class='bts_qty_" + sbcount + " bts_qntt form-control text-right' placeholder='0' value='" + BStockOut + "' min='0' data-max='" + stmax + "' required='required' /></td>" +
            "<td class='text-center'><button data-count='" + sbcount + "' class='btn btn-danger' type='button' value='Delete'  onclick='deleteBtsRows(this,\"" + arg + "\")'><i class='fa fa-trash fa-1x'></i></button>" +
            "<input type='hidden' data-name='Item' class='bts_item' value='" + ItemId + "'/>" +
            "<input type='hidden' data-name='cfactor' class='bts_cfactor' value='" + cfactor + "'/>" +
            "<input type='hidden' data-name='Priunit' class='bts_punit' value='" + punit + "'/>" +
            "<input type='hidden' data-name='Secunit' class='bts_sunit' value='" + sunit + "'/>" +
            "<input type='hidden' data-name='Cost' class='bts_cost'  value='" + sbtcost + "'/>" +
            "<input type='hidden' data-name='Unit' class='bts_units' id='bts_unit_name_" + sbcount + "' value='" + sbunit + "'>" +
            "<input type='hidden' data-name='Order' class='bts_order'  value='" + border + "'/>" +
            "</td>";
        row += data + "</tr>";
        $('#ubatchtbl-' + arg + ' tbody').append(row);
        sbcount++;
        totalbtsqty(arg);
        $('.date').datepicker({
            format: 'dd-mm-yyyy',
            autoclose: true,
            allowInputToggle: true,
        });
        BatchNo(ItemId);
    }
}

function BatchNo(item) {
    // Item Category
    $(".bts_batchno").select2({
        placeholder: 'Search Batch No',
        minimumInputLength: 0,
        ajax: {
            url: "/Item/SearchBatch",
            dataType: 'json',
            type: "POST",
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 1,
                    itemid: $(this).attr("data-item")
                };
            },
            processResults: function (data, params) {
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
        templateResult: batchFormatResult,
        templateSelection: batchFormatSelection,
    });
}
function batchFormatResult(repo) {
    var bg = "";
    var markup = '<div class="se-row' + bg + '">' +
        '<h4>' + repo.text + '</h4>';
    markup += '<div class="se-sec">Stock:' + parseFloat(repo.Stock).toFixed(2) + '</div>';
    if (repo.Stock) {

    }


    markup += '</div>';
    var retn = $(markup);
    return retn;
}

function batchFormatSelection(repo) {
    return repo.text;
}

function BStcostups(arg) {
    var gtTot = 0;
    $("#uitem_" + arg + " .utotrate").each(function () {
        var rate = $(this).val();
        rate = rate || 0;
        gtTot = parseFloat(gtTot) + parseFloat(rate);
    });
    var sbtcost = gtTot / $("#uitem_" + arg + " .utotrate").length;
    $("#ubatchtbl-" + arg + " .bts_cost").val(sbtcost);
}
function btsBtch_change(t, arg, itemid) {
    var BatchNo = $(t).val();
    $.ajax({
        url: "/Item/GetBatch",
        type: "GET",
        dataType: "JSON",
        data: { BatchNo: BatchNo, itemid: itemid },
        success: function (result) {
            var gp = $(t).parents("tr");
            gp.find('.bts_mfgdate').val(convertToDate(result.MFG));
            gp.find('.bts_expdate').val(convertToDate(result.EXP));
            var VoucherType = $("#VoucherType").val();
            if (VoucherType == "Sales") {
                var BID = "#batch-" + arg;
                var punit = $(BID + " .BStUnit").attr('data-ItemUnitID');
                var sbunit = parseFloat($('#uunit_name_' + arg).val());
                var cfactor = $(BID + " .BStUnit").attr('data-confactor');
                var BStock = (sbunit != punit) ? BtData.Stock : (parseFloat(BtData.Stock) / parseFloat(cfactor));



                var dataMax = parseFloat(gp.find('.bts_qntt').attr('data-max')) + parseFloat(BStock);
                var max = gp.find('.bts_qntt').attr('max', dataMax);
            }
        }
    });
    btsqty_changez(t, arg, itemid);
}
function btsqty_changez(t, arg, itemid) {
    var barg = $(t).attr('data-count');
    var flag = "";

    $("#ubatchtbl-" + arg + " tr").each(function () {
        var batch = $(this).find(".bts_batchno").val();
        var qty = $(this).find('.bts_qntt').val();
        if (batch == "" || qty <= 0) {
            flag = "nop";
        }
    });
    if (flag != "nop") {
        var VoucherType = $("#VoucherType").val();
        if (VoucherType == "Purchase" || VoucherType == "SalesReturn") {
            addSBPURRows(arg, itemid);
        }
        if (VoucherType == "Sales" || VoucherType == "PurchaseReturn") {
            addSBSalRows(arg, itemid);
        }
    }
    var gp = $(t).parents("tr");
    var max = parseFloat(gp.find('.bts_qntt').attr('max'));
    var min = parseFloat(gp.find('.bts_qntt').attr('min'));
    var btsQty = parseFloat(gp.find('.bts_qntt').val());
    if (btsQty > max) {
        gp.find('.bts_qntt').val(max);
    }
    else if (btsQty < min) {
        gp.find('.bts_qntt').val(min);
    }
    totalbtsqty(arg);
}
function totalbtsqty(arg) {
    var btsqty = 0;
    $("#ubatchtbl-" + arg + " tr").each(function () {
        var bqty = $(this).find('.bts_qntt').val();
        var batch = $(this).find(".bts_batchno").val();
        bqty = bqty || 0;
        btsqty += (batch != "") ? parseFloat(bqty) : 0;
    });
    $("#ubatchtbl-" + arg + " .bstotqty").text(btsqty.toFixed(2));
}

function btsclose(arg) {

    $('#batch-' + arg + '').modal('hide');
}
function btsSubmits(arg) {
    var btsqty = $("#ubts_tqty_" + arg).text();
    var itemqty = 0;
    $("#ubatchtbl-" + arg + " tr").each(function () {
        var bqty = $(this).find('.bts_qntt').val();
        var batch = $(this).find(".bts_batchno").val();
        bqty = bqty || 0;
        itemqty += (batch != "") ? parseFloat(bqty) : 0;
    });
    if (btsqty != itemqty) {
        alert("Item Quantity and Total batch wise Quantity should be Same ..!! ");
    } else {
        $('#batch-' + arg + '').modal('hide');
    }
}

function deleteBtsRows(t, arg) {
    var barg = $(t).attr('data-count');
    var batch = $("#ubatchtbl-" + arg + " .bts_batchno_" + barg).val();
    var qty = $("#ubatchtbl-" + arg + " .bts_qty_" + barg).val();
    if (batch != "" && qty > 0) {
        var e = t.parentNode.parentNode;
        e.parentNode.removeChild(e);
    }
    else {
        alert("Sorry You Can't Delete This Row.");
    }
    totalbtsqty(arg);
}

function quantity_changeU(arg) {

    //if ($('total_qntt_' + arg).val() > 0) {
    minstockcheckU(arg);
    rowSubTotalU(arg);
    CalculatetblItemListSumU();
    grandtotalcalculationU();
    paidamountcalculationU();
    // batchstock update
    var tqty = $("#utotal_qntt_" + arg).val();

    $("#ubts_tqty_" + arg).text(tqty);
    var ItemId = $("#uitem_name_" + arg).val();
    var VoucherType = $("#VoucherType").val();
    if (VoucherType == "Purchase" || VoucherType == "Sales" || VoucherType == "SalesReturn" || VoucherType == "PurchaseReturn") {

        // PopupBatchStocks(arg, ItemId);
        BStcostups(arg);
    }

}
function rate_changeU(arg, type, foredit) {
    minstockcheckU(arg);
    var baserate = $("#ubase_rate_" + arg).val();
    var rate = $(".uprice_item_" + arg).val();

    var cfactor = parseFloat($('#cfactor_' + arg).val());

    var purvalue = baserate;// $("#selitem_pprice_" + arg).val();

    if (parseFloat(baserate) > parseFloat(rate) && type == 'sales' && parseFloat(rate) > 0 && foredit != 'foredit') {
        // alert("Selling price is less than Base Price ");
    }
    var index = $('#uunit_name_' + arg).prop('selectedIndex');
    var keepstock = $(".uminstock_" + arg).attr('data-keeps')
    if (keepstock == "yes") {
        if (index == 1) {
            var ConvrtdPrice = (purvalue / cfactor).toFixed(2);
            if ((parseFloat(rate) < parseFloat(ConvrtdPrice)) && parseFloat(rate) > 0) {
                alert("Selling price is less than Purchase Price : " + (ConvrtdPrice));
               // $(".uprice_item_" + arg).val("");
            }
        }
        else {
            if ((parseFloat(rate) < parseFloat(purvalue)) && parseFloat(rate) > 0) {
                alert("Selling price is less than Purchase Price : " + purvalue);
               // $(".uprice_item_" + arg).val("");
            }
        }
    }
    rowSubTotalU(arg);
    CalculatetblItemListSumU();
    grandtotalcalculationU();
    paidamountcalculationU();
}
function itemdiscount_changeU(arg) {
    rowSubTotalU(arg);
    CalculatetblItemListSumU();
    grandtotalcalculationU();
    paidamountcalculationU();
}

//function discount_change(arg) {
//    grandtotalcalculation();
//}
function paidamount_changeU() {
    //CalculatetblItemListSum();
    paidamountcalculationU();
}

function rowSubTotalU(arg) {
    var tax = $("#utax_percentage_" + arg).val();
    var quantity = $(".utotal_qntt_" + arg).val();
    var rate = $(".uprice_item_" + arg).val();
    //alert("price : " + rate + " arg =" + arg);
    var subtotal = quantity * rate;
    $(".usub_total_" + arg).val(subtotal.toFixed(2));
    var itemdiscount = $(".uitem_discount" + arg).val();
    subtotal = subtotal - itemdiscount;

    var taxAmount = subtotal * (tax / 100);
    var Total = subtotal + taxAmount;

    $("#utot_tax_" + arg).val(taxAmount.toFixed(2));
    $(".utax_" + arg).val(taxAmount.toFixed(2) + " (" + tax + "%)");
    $(".utotal_price_" + arg).val(Total.toFixed(2));
}

function paidamountcalculationU() {
    var paidAmt = $("#UMPaidAmount").val();
    var gdTotal = $("#UMGrandTotal").val();
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
function CalculatetblItemListSumU() {
    var tax = $(".tot_tax").val();
    var qty = $(".ItemQty").val();
    if (tax > 0 || qty != 0) {
        var tbody = $("#normalinvoiceused tbody");
        if (tbody.children().length > 0) {
            var gtTax = 0;
            var gtTotal = 0;
            var gtQty = 0;
            var gtSubTotal = 0;
            var gtDiscount = 0;
            var gtRate = 0;
            $(".utot_tax").each(function () {
                var indTax = $(this).val();

                gtTax = parseFloat(gtTax) + parseFloat(indTax);
            });

            $("[id$=total_tax_amountR]").text(parseFloat(gtTax).toFixed(2));
            $(".utotal_price").each(function () {
                var indtot = $(this).val();
                gtTotal = parseFloat(gtTotal) + parseFloat(indtot);
            });
            $(".uquty").each(function () {
                var subQty = this.value;
                gtQty = parseFloat(gtQty) + parseFloat(subQty);
            });

            $(".usubtotal").each(function () {
                var subTot = this.value;
                gtSubTotal = parseFloat(gtSubTotal) + parseFloat(subTot);
            });
            $(".uitem_discount").each(function () {
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
            $("[id$=totalU]").text((gtTotal).toFixed(2));
            $("[id$=ItemCountU]").val(tbody.children().length);
            $("[id$=ItemQtyU]").text((gtQty).toFixed(2));
            $("[id$=SubTotalU]").text((gtSubTotal).toFixed(2));
            $("[id$=ItemDiscU]").text(parseFloat(gtDiscount).toFixed(2));
        }
    }
}
//item unit change
function unitchangeU(selectObject, arg, action) {
    minstockcheckU(arg);
    var index = $('#uunit_name_' + arg).prop('selectedIndex');

    if (index == 1) {
        var unitId = parseFloat($('#uunit_name_' + arg).val());
        var cfactor = parseFloat($('#ucfactor_' + arg).val());
        var price = parseFloat($("#ubase_rate_" + arg).attr("data-value"));
        var newprice = parseFloat(price / cfactor);
        $(".uprice_item_" + arg).val(newprice.toFixed(2));
    } else {
        var unitId = parseFloat($('#uunit_name_' + arg).val());
        //var cfactor = parseFloat($('#cfactor_' + arg).val());
        var price = parseFloat($("#ubase_rate_" + arg).attr("data-value"));
        //var newprice = parseFloat(price * cfactor);
        $(".uprice_item_" + arg).val(price.toFixed(2));
    }

    rowSubTotalU(arg);
    CalculatetblItemListSumU();
    grandtotalcalculationU();
    paidamountcalculationU();
}

//Delete a row of table
function deleteRowU(t) {
    var classname = $(t).closest('tr').attr('class');
    if (classname == 'uitem_') alert("Sorry you can't delete this row.");
    else {
        var r = confirm("Are you sure you want to delete this..?");
        if (r == true) {
            var e = t.parentNode.parentNode;
            e.parentNode.removeChild(e);
        }
    }
    CalculatetblItemListSumU();
    grandtotalcalculationU();
    paidamountcalculationU();
    var i = 1;
    $('#addinvoiceusedpopup tr').each(function () {
        $(this).find('td:first').text(i);
        i++;
    });
}

//percentage cal on billsundry
function calculatePercentageU(dataid) {
    var total = parseFloat($("#total").text());
    total = (total > 0) ? total : 0;
    var value = parseFloat($("#ubsvalue_" + dataid).val());
    var amt = (total * (value / 100));
    $("#ubsamt_" + dataid).val(amt.toFixed(2));
}

function BindBsAmountU(dataid, defvalue) {
    var value = parseFloat($("#ubsvalue_" + dataid).val());
    var bstype = parseFloat($("#ubstype_" + dataid).val());
    var amtype = $("#uamttypevalue_" + dataid).val();
    var total = parseFloat($("#utotal").text());


    total = (total > 0) ? total : 0;
    if (amtype == 0) {
        $("#ubsvalue_" + dataid).val("").attr('readonly', true);
        $("#ubsamttype_" + dataid).val("");
        $("#ubsamt_" + dataid).val(parseFloat(defvalue).toFixed(2));
        $("#ubsamt_" + dataid).focus();
    } else {
        $("#ubsvalue_" + dataid).focus();
        $("#ubsvalue_" + dataid).val(defvalue);
        $("#ubsamttype_" + dataid).val("%");
        $("#ubsvalue_" + dataid).attr('readonly', false);
        calculatePercentage(dataid);
    }
    grandtotalcalculationU();
}
function grandtotalcalculationU() {
    var gtTotal = parseFloat($("#totalU").text());
    gtTotal = (gtTotal > 0) ? gtTotal : 0;

    //FCCalculationR();
    paidamountcalculationU();
}

//onchange of billsundry value
function bsvaluechangeU(arg) {
    var defvalue = $("#ubsvalue_" + arg).val();
    BindBsAmountU(arg, defvalue);
}
//amt chnage
function bsamtchangeU(arg) {
    var defvalue = parseFloat($("#ubsamt_" + arg).val());
    BindBsAmountU(arg, defvalue);
}
function bspcalculate() {
    var gtTotal = parseFloat($("#total").text());
    var taxAmt = parseFloat($("#total_tax_amount").text());

    var subtot = 0;
    var stype = (act == "sales") ? $("#SalesType").val() : ((act == "purchase") ? $("#PurchaseType").val() : $("#SalesType").val());//voucher wise tax
    if (stype == 3) {
        subtot = parseFloat(gtTotal) - parseFloat(taxAmt);
    } else {
        subtot = parseFloat(gtTotal);
    }
    subtot = subtot - discountglog;
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
//Delete a row of table
function deletebsRowU(t) {
    var classname = $(t).closest('tr').attr('class');
    if (classname == 'ubs_') alert("Sorry You Can't Delete This Row.");
    else {
        var r = confirm("Are you sure you want to delete this..?");
        if (r == true) {
            var e = t.parentNode.parentNode;
            e.parentNode.removeChild(e);
        }
    }
}

//print item bill sundry
//itembind
function bindItemU(e, dvitem) {
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
    function ItemsBindU(uitem, utype, bcount) {
        var Row = "";
        var unit = (uitem.ItemUnit != null) ? uitem.ItemUnit : "";
        var PartNo = (uitem.PartNumber != null && uitem.PartNumber != "") ? uitem.PartNumber : "";
        var itemnote = "";
        if (uitem.ItemNote != "" && uitem.ItemNote != "-:{Bundle_Item}") {
            itemnote = "<br /><small>" + uitem.ItemNote + "</small>";
        }
        var dvField1 = "";
        var dvField2 = "";
        var trcount = rtype == "bundle" ? bcount : count;
        if (dvitem != "active" && rtype != "bundle") {
            dvField1 += '<td class="text-right"><b>' + parseFloat(uitem.ItemUnitPrice).toFixed(2) + '</b></td>';
            dvField1 += '<td class="text-right"><b>' + parseFloat(uitem.ItemSubTotal).toFixed(2) + '</b></td>';
            dvField2 += '<td class="text-right">' + parseFloat(uitem.ItemTaxAmount).toFixed(2) + '</td>';
            dvField2 += '<td class="text-right">' + parseFloat(uitem.ItemTotalAmount).toFixed(2) + '</td>';
        } else if (dvitem != "active" && rtype == "bundle") {
            dvField1 += '<td></td><td></td>';
            dvField2 += '<td></td><td></td>';
        }
        Row += '<tr class="border-top">';
        Row += '<td>' + trcount + '</td>';
        if (uitem.PNoStatus == 0) {
            $("#PoNo").show();
            Row += '<td>' + PartNo + '</td>';
        }
        // Default Invoice Structure
        if (Layout == "Default") {
            if (e.summary.chkCode == 0) {
                itemcode = uitem.ItemCode + " - ";
            }
            Row += '<td>' + itemcode + uitem.ItemName + itemnote + '</td>';
            Row += '<td>' + unit + '</td>';
            Row += '<td>' + uitem.ItemQuantity + '</td>';
            Row += dvField1 + dvField2;
        }
        else if (Layout == "Jewellery") {
            Row += '<td>' + uitem.ItemCode + '</td>';
            Row += '<td>' + uitem.ItemName + itemnote + '</td>';
            Row += '<td>' + uitem.ItemQuantity + '</td>';
            Row += '<td>' + uitem.ItemQuantity + '</td>';
            Row += dvField1;
        }
        else if (Layout == "Scaffold") {
            var CBM = (uitem.CBM != null && uitem.CBM != "") ? (parseFloat(uitem.CBM) * parseFloat(uitem.ItemQuantity)).toFixed(3) : "";
            var Weight = (uitem.Weight != null && uitem.Weight != "") ? (parseFloat(uitem.Weight) * parseFloat(uitem.ItemQuantity)).toFixed(3) : "";
            var img = "";
            wgt = parseFloat(wgt) + parseFloat(Weight || 0);
            cbm = parseFloat(cbm) + parseFloat(CBM || 0); if (uitem.img != null && uitem.img.length > 0) {
                $.each(uitem.img, function (j, imgs) {
                    var im = "/uploads/itemimages/" + uitem.Id + "/thumb_" + imgs.FileName;
                    img = "<img width='68' height='46' src='/uploads/itemimages/" + uitem.Id + "/thumb_" + imgs.FileName + "'/>";
                    // img = "<div style='width:50px;height:50px;background:url(" + im + ");background-size: cover;'></div>";
                });
                if (rtype != "bundle") {
                    Row += '<td><b>' + uitem.ItemName + "</b>" + itemnote + '</td>';
                } else {
                    Row += '<td>' + uitem.ItemName + itemnote + '</td>';
                }

                Row += '<td style="width:70px; padding:1px;">' + img + '</td>';
            }
            else {
                if (rtype != "bundle") {
                    Row += '<td colspan="2"><b>' + uitem.ItemName + "</b>" + itemnote + '</td>';
                } else {
                    Row += '<td colspan="2">' + uitem.ItemName + itemnote + '</td>';
                }
            }
            if (rtype != "bundle") {
                Row += '<td><b>' + Weight + '</b></td>';
                Row += '<td><b>' + CBM + '</b></td>';
                Row += '<td><b>' + uitem.ItemQuantity + ' ' + unit + '</b></td>';
            } else {
                Row += '<td>' + Weight + '</td>';
                Row += '<td>' + CBM + '</td>';
                Row += '<td>' + uitem.ItemQuantity + ' ' + unit + '</td>';

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
function bindSundryU(e) {
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
function coFactorChangeU() {
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
function AddItempopup() {
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


function setTabIndexU() {
    var j = 1;
    $('body').find('input,textarea,select,button, .select2-container .select2-selection__rendered').not(".select2-hidden-accessible").not(":hidden").each(function (i) {
        if (!$(this).hasClass("select2-hidden-accessible") && !$(this).is(":hidden")) {
            $(this).attr('tabindex', j);
            j++;
        }
        if ($(this).closest("tr").hasClass("uitem_") && !$(this).hasClass("select2-selection__rendered")) {
            $(this).attr('tabindex', -1);
        }
    });
}

function UsedMaterialsPopUp() {

    $.fn.modal.Constructor.prototype.enforceFocus = function () { };
    $('#modal-UsedMaterials').on('shown.bs.modal', function (e) {
        $("#ddlUMC").select2({
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

    $('#ddlUMC').select2({
        dropdownParent: $('#modal-usedmaterials')
    });

    //$.ajax({
    //    url: '/CreditSaleReturn/GetMaxReturnId',
    //    type: "GET",
    //    dataType: "JSON",
    //    success: function (result) {
    //        $("#UsedmaterialsId").val(result);
    //    }
    //});

    var data = $('#ddlCustomer').select2('data');
    $("#UsedCustomer").val(data[0].text);


    var tbody1 = $("#normalinvoiceused tbody");
    if (tbody1.children().length == 0) {
        addrowused('addinvoiceusedpopup', 'sales', "", "0.00", "0.00", "0");
    }

    //$('#addinvoiceusedpopup').html('');
    //$('#addbillsundrypopup').html('');
    $("#modal-usedmaterials").modal({ show: true, backdrop: "static" });



}