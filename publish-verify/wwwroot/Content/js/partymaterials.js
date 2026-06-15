var count = 1, type = '';
limits = 500;
//Add Row
function addrow(t, action, Unit, Quantity, Price, Amount, Item, ItemName, ItemWithCode, ItemNote, itemdata, ConFactor) {
    if (count == limits) alert("You have reached the limit of adding " + count + " inputs");
    else {
        var Option = "";
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
        tab1 = tabindex + 1;
        tab2 = tabindex + 2;
        tab3 = tabindex + 3;
        tab4 = tabindex + 4;
        tab5 = tabindex + 5;
        tab6 = tabindex + 6;
        tab7 = tabindex + 7;

        if (Item != null) {
            row = "<tr class='item_" + Item + "' id='item_" + count + "'>";
            Option = "<option value='" + Item + "'>" + ItemWithCode + "</option>";
        }
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
            '<div class="form-group"><textarea name="itemnote" cols="40" rows="10" class="form-control itemnote" id="itemnote-' + count + '" maxlength="255">' + inote + '</textarea></div>' +
            '<div class="form-group"><button class="btn btn-info" type="button" data-dismiss="modal">Save</button></div>' +
            '</div></div></div>';
        notbtn = "<button type='button' class='itnote btn btn-default btn-flat' data-toggle='modal' data-target='#modal-item-" + count + "'><i class='fa fa-1x fa-file-text-o'></i></button>";
        // }
        if (itemdata) {
            if (type == "foredit") {
                price = itemdata.PurchasePrice;
            }
            
                htdata = "<div class='minstock_" + count + "'";
                if (itemdata.KeepStock == true) {
                    var qntmin = 0;
                    if (itemdata.Unit == itemdata.ItemUnitID) {
                        qntmin = Quantity * itemdata.ConFactor;
                    }
                    if (itemdata.Unit == itemdata.SubUnitId) {
                        qntmin = Quantity;
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
        var itemaddbtn = "<span class='input-group-btn'><a type='button' href='/Item/AddItem' class='modal-create-lg btn btn-success btn-flat'><i class='fa fa-1x fa-plus-circle'></i></a>" + notbtn + "</span>";
        
        //ItemDiscount = ItemDiscount != null ? ItemDiscount : 0;

        //data = "<td class='text-center' id=" + divid + "> " + slno + " </td>" +
        //         "<td class='input-group input-group-sm'><select class='form-control item_name' " + required + " data-id='" + count + "' placeholder='Item Name' data-msg-required='The Item Name is required' id='item_name_" + count + "'  data-val-required='The Item field is required' onchange='GetItemdetails(this," + count + ",\"" + type + "\")'>" + Option + "</select> " + itemaddbtn + "</td>" +
        //         "<td style='width:100px;'><select class='form-control units unit_name_" + count +"' id='unit_name_" + count + "' " + required + " data-id='" + count + "' id='unit_name' onchange='unitchange(this," + count + ",\"" + type + "\");'></select></td>" +
        //         "<td> <input type='number' name='product_quantity[]' onchange='quantity_change(" + count + ");' id='total_qntt_" + count + "' value='" + ItemQuantity + "'  class='total_qntt_" + count + " form-control text-right quty' placeholder='0' min='0' tabindex='" + tab2 + "'/></td>" +
        //         "<td><input type='number' data-msg-required='The Item Rate is required' name='product_rate[]' " + required + " onchange='rate_change(" + count + ",\"" + type + "\");' id='price_item_" + count + "' value='" + ItemUnitPrice + "' class='price_item_" + count + " form-control text-right totrate' placeholder='0.00' min='0' tabindex='" + tab3 + "'/><input type='hidden' data-value='" + price + "' value='" + baseprice + "' name='base_rate' id='base_rate_" + count + "'></td>" +
        //         "<td><input type='number' name='sub_total[]' id='sub_total_" + count + "' class='sub_total_" + count + " form-control text-right subtotal' value='" + ItemSubTotal + "'   placeholder='0.00' min='0' tabindex='" + tab3 + "' readonly='readonly'/>" +
        //         "<input type='hidden' name='item_discount[]' id='item_discount" + count + "' onchange='itemdiscount_change(" + count + ");' class='item_discount" + count + " form-control text-right item_discount' value='" + ItemDiscount + "' value='0.00' placeholder='0.00' tabindex='" + tab3 + "'/></td>" + "</td>" +
        //         "<input type='hidden' id='tax_" + count + "' class='form-control text-right tax tax_" + count + "' tabindex='" + tab4 + "' readonly='readonly' /><input type='hidden' class='item_amount' name='item_amount' id='item_amount_" + count + "'/><input type='hidden' class='tot_tax' name='tot_tax' id='tot_tax_" + count + "'/><input type='hidden'  class='tax_percentage' value='" + ItemTax + "' name='tax_percentage' id='tax_percentage_" + count + "'/></td>" +
        //         "<input class='total_price total_price_" + count + " form-control text-right' type='hidden' name='total_price[]' value='" + ItemTotalAmount + "' id='total_price_" + count + "' value='0.00' readonly='readonly'/><input type='hidden' class='cfactor' name='cfactor' id='cfactor_" + count + "'/>" +
        //         "<td class='text-center'><button tabindex='" + tab5 + "' style='text-align: right;' class='btn btn-danger' type='button' value='Delete' onclick='deleteRow(this)'><i class='fa fa-trash fa-1x'></i></button>" + itemnote + htdata + "</td>";
        //row += data + "</tr>";

        data = "<td class='text-center' id=" + divid + "> " + slno + " </td>" +
                "<td class='input-group input-group-sm'><select class='form-control item_name' " + required + " data-id='" + count + "' placeholder='Item Name' data-msg-required='The Item Name is required' id='item_name_" + count + "'  data-val-required='The Item field is required' onchange='GetItemdetails(this," + count + ",\"" + type + "\")'>" + Option + "</select> " + itemaddbtn + "</td>" +
                "<td style='width:100px;'><select class='form-control units unit_name_" + count + "' id='unit_name_" + count + "' " + required + " data-id='" + count + "' id='unit_name' onchange='unitchange(this," + count + ",\"" + type + "\");'>''</select></td>" +
                "<td><input type='number' name='product_quantity[]' onchange='quantity_change(" + count + ");' id='total_qntt_" + count + "' value='" + Quantity + "'  class='total_qntt_" + count + " form-control text-right quty' placeholder='Enter Quantity' min='0' tabindex='" + tab2 + "'/></td>" +
                "<td><input type='number' data-msg-required='The Item Rate is required' name='product_rate[]' " + required + " onchange='rate_change(" + count + ",\"" + type + "\");' readonly id='price_item_" + count + "' value='" + Price + "' class='price_item_" + count + " form-control text-right totrate' placeholder='0.00' min='0' tabindex='" + tab3 + "'/><input type='hidden' data-value='" + price + "' value='" + baseprice + "' name='base_rate' id='base_rate_" + count + "'></td>" +
                "<td><input type='number' name='sub_total[]' id='sub_total_" + count + "' class='sub_total_" + count + " form-control text-right subtotal' value='" + Amount + "'   placeholder='0.00' min='0' tabindex='" + tab3 + "' readonly='readonly'/><input type='hidden' value='" + ConFactor + "' class='cfactor' name='cfactor' id='cfactor_" + count + "'/>" +  
                "<td class='text-center'><button tabindex='" + tab5 + "' style='text-align: right;' class='btn btn-danger' type='button' value='Delete' onclick='deleteRow(this)'><i class='fa fa-trash fa-1x'></i></button>" + itemnote + htdata + "</td>";
        row += data + "</tr>";
        $('#' + t).append(row);
        //$('#item_ .item_name').focus();
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
        //$('#total_qntt_' + dataid).val(1);
        if (ItemId != null) {
            if ($(".item_" + ItemId).length > 0) {
                if ($(".item_" + ItemId).length < 4) {
                    if (confirm('Are you sure want to Add this item Again?')) {
                        itemUpdate(selectObject, dataid, action);
                    }
                    else {
                        $('#item_name_' + dataid).val(null).trigger('change');
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
function itemUpdate(selectObject, dataid, action,unitid) {
    var mc = $("#ddlMCFrom").val();
    var ROnlyRate = $("#ROnlyRate").val();
    if (ROnlyRate == "active") {
        $("#price_item_" + dataid).attr('readonly', true);
    }
    $.ajax({
        url: '/Item/GetItemMC',
        type: "GET",
        dataType: "JSON",
        data: { itemID: selectObject.value, mc: mc },
        success: function (result) {
            if (action != "sales" || (action == "sales" && result.KeepStock != true) || (action == "sales" && result.KeepStock == true && result.total > 0)||(mc == 20093 || mc == 20094 || mc == 20095)) {
                // append item unit list
                createUnitList(result, dataid);
              
                //if (action == "sales" || action == "quot") {
                //    $(".price_item_" + dataid).val(result.SellingPrice);
                //    $("#item_amount_" + dataid).val(result.SellingPrice);
                //    $("#base_rate_" + dataid).attr("data-value", result.SellingPrice);
                //    $("#total_qntt_" + dataid).val(1);
                //}
                //if (action == "purchase") {
                            
                if (result.stocktransfernotreadonly == true) {
                    $(".totrate").prop("readonly", false);
                }
                    $(".price_item_" + dataid).val(result.PurchasePrice);
                    $("#item_amount_" + dataid).val(result.PurchasePrice);
                    $("#base_rate_" + dataid).attr("data-value", result.PurchasePrice);
                    $("#total_qntt_" + dataid).val(1);
                //}

                //$("#total_qntt_" + dataid).val(result.ItemQuantity);
                //set minimum value and message
                $("#total_qntt_" + dataid).attr('min', "0.01");
                $("#total_qntt_" + dataid).attr('data-msg-min', "Item Quantity Must Be Greater Than 1");

                //$("#tax_percentage_" + dataid).val(result.Tax);
                $("#base_rate_" + dataid).val(result.BasePrice);

                $("#cfactor_" + dataid).val(result.ConFactor);

                rowSubTotal(dataid);
                CalculatetblItemListSum();
                grandtotalcalculation();
                paidamountcalculation();
                $(selectObject).closest('tr').attr('class', "item_" + result.ItemID);
                if (action == "sales") {
                    minstockupdate(result, dataid);
                }
                if ($(".item_").length == 0) {
                    addrow('addinvoiceItem', '', '', '0.00', '0.00', '0');
                }
                $('.unit_name_' + dataid).focus();
            } 
            else if ((result.KeepStock == true && result.CheckStock == 0 && result.total <= 0) ) {

                var res = confirm("Are you Sure Want To Add Items In Less Stock ?");
                if (res == true) {
                    $(selectObject).closest('tr').attr('class', "item_" + result.ItemID);
                    if (action == "sales") {
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
                    $("#total_qntt_" + dataid).val(0).trigger("change");
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
                }
            }
        }
    });
}
//check minimum stock
function itemUpdateunit(selectObject, dataid, action) {
    var mc = $("#ddlMCFrom").val();
    var ROnlyRate = $("#ROnlyRate").val();
    if (ROnlyRate == "active") {
        $("#price_item_" + dataid).attr('readonly', true);
    }
    $.ajax({
        url: '/Item/GetItemMC',
        type: "GET",
        dataType: "JSON",
        data: { itemID: selectObject.value, mc: mc },
        success: function (result) {
            if (action != "sales" || (action == "sales" && result.KeepStock != true) || (action == "sales" && result.KeepStock == true && result.total > 0)) {
                // append item unit list
                createUnitList(result, dataid);
                $("#unit_name_" + dataid).val(unitid);
                //if (action == "sales" || action == "quot") {
                //    $(".price_item_" + dataid).val(result.SellingPrice);
                //    $("#item_amount_" + dataid).val(result.SellingPrice);
                //    $("#base_rate_" + dataid).attr("data-value", result.SellingPrice);
                //    $("#total_qntt_" + dataid).val(1);
                //}
                //if (action == "purchase") {
                //$(".price_item_" + dataid).val(result.PurchasePrice);
                //$("#item_amount_" + dataid).val(result.PurchasePrice);
                //$("#base_rate_" + dataid).attr("data-value", result.PurchasePrice);
                //$("#total_qntt_" + dataid).val(1);
                ////}

                ////$("#total_qntt_" + dataid).val(result.ItemQuantity);
                ////set minimum value and message
                //$("#total_qntt_" + dataid).attr('min', "0.01");
                //$("#total_qntt_" + dataid).attr('data-msg-min', "Item Quantity Must Be Greater Than 1");

                //$("#tax_percentage_" + dataid).val(result.Tax);
                $("#base_rate_" + dataid).val(result.BasePrice);

                $("#cfactor_" + dataid).val(result.ConFactor);

                //rowSubTotal(dataid);
                //CalculatetblItemListSum();
                //grandtotalcalculation();
                //paidamountcalculation();
                //$(selectObject).closest('tr').attr('class', "item_" + result.ItemID);
                //if (action == "sales") {
                //    minstockupdate(result, dataid);
                //}
                //if ($(".item_").length == 0) {
                //    addrow('addinvoiceItem', '', '', '0.00', '0.00', '0');
                //}
                //$('.unit_name_' + dataid).focus();
            }
            else if ((result.KeepStock == true && result.CheckStock == 0 && result.total <= 0)) {

                var res = confirm("Are you Sure Want To Add Items In Less Stock ?");
                if (res == true) {
                    $(selectObject).closest('tr').attr('class', "item_" + result.ItemID);
                    if (action == "sales") {
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
                    $("#total_qntt_" + dataid).val(0).trigger("change");
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
                }
            }
        }
    });
}
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
    var mc = $("#ddlMCFrom").val();
    if (mc == 20093 || mc == 20094 || mc == 20095) {
        return true;
    }
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
function createUnitList(result, dataid) {
    var ROnlyRate = $("#ROnlyRate").val();
    if (ROnlyRate == "active") {
        $("#price_item_" + dataid).attr('readonly', true);
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
            url: "/Item/Search",
            dataType: 'json',
            type: "POST",
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0,
                    //x: "All"
                };
            },
            results: function (data) {
                return { results: data };
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
        templateResult: repoFormatResult,
        templateSelection: repoFormatSelection,
    });
    rate_change(count);
}


function repoFormatResult(repo) {
    var bg = "";
    if (repo.KeepStock) {
        bg = (parseFloat(repo.total) > 0) ? "" : " text-red";
    }
    var markup = '<div class="se-row' + bg + '">' +
             '<h4>' + repo.text + '</h4>';
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
    if ($('#item_name_' + arg).val() != null) {
        minstockcheck(arg);
        rowSubTotal(arg);
        CalculatetblItemListSum();
        grandtotalcalculation();
        paidamountcalculation();
    }
    else {
        $('#total_qntt_' + arg).val(0);
        $('#price_item_' + arg).val(0);
    }
}
function rate_change(arg, type, foredit) {
    if ($('#item_name_' + arg).val() != null) {
        minstockcheck(arg);
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
    else {
        $('#total_qntt_' + arg).val(0);
        $('#price_item_' + arg).val(0);
    }
}
function itemdiscount_change(arg) {
    rowSubTotal(arg);
    CalculatetblItemListSum();
    grandtotalcalculation();
    paidamountcalculation();
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
    var subtotal = quantity * rate;
    $(".sub_total_" + arg).val(subtotal.toFixed(2));
    var itemdiscount = $(".item_discount" + arg).val();
    subtotal = subtotal - itemdiscount;

    var taxAmount = subtotal * (tax / 100);
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
    //alert($("#tot_tax_1").val());

    var tax = $(".tot_tax").val();
    var qty = $(".quty").val();

    //if (tax > 0 || qty != 0) {
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

        $(".totrate").each(function () {            
            var ttr = $(this).val();
            ttr = ttr || 0;
            gtRate = parseFloat(gtRate) + parseFloat(ttr);            
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

        // $("#GrandTotal").val(parseFloat(gtTotal).toFixed(2));
        $("[id$=ToItemPrice]").text((gtRate).toFixed(2));
        $("[id$=total]").text((gtTotal).toFixed(2));
        $("[id$=ToItemCount]").text(tbody.children().length - 1);
        $("[id$=ToItemQnt]").text((gtQty).toFixed(2));
        $("[id$=ToItemAmount]").text((gtSubTotal).toFixed(2));
        $("[id$=ItemDisc]").text(parseFloat(gtDiscount).toFixed(2));
        //$("[id$=TotalAmount]").text((gtSubTotal).toFixed(2));
    }
    //  }
}
//item unit change
function unitchange(selectObject, arg, action) {
    minstockcheck(arg);
    var index = $('#unit_name_' + arg).prop('selectedIndex');
    //var itemobj = document.getElementById('item_name_' + arg);
   
    

    if (index == 1) {
        var unitId = parseFloat($('#unit_name_' + arg).val());
        var cfactor = parseFloat($('#cfactor_' + arg).val());
        var price = parseFloat($("#base_rate_" + arg).attr("data-value"));
        var newprice = parseFloat(price / cfactor);
        $(".price_item_" + arg).val(newprice.toFixed(2));
    } else {
        var unitId = parseFloat($('#unit_name_' + arg).val());
        var cfactor = parseFloat($('#cfactor_' + arg).val());
        var price = parseFloat($("#base_rate_" + arg).attr("data-value"));
        var newprice = parseFloat(price * cfactor);
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
    var total = parseFloat($("#total").text());
    total = (total > 0) ? total : 0;
    var value = parseFloat($("#bsvalue_" + dataid).val());
    var amt = (total * (value / 100));
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
    var gtTotal = parseFloat($("#ToItemAmount").text());
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
    $("#GrandTotal").val(parseFloat(gtTotal).toFixed(2));
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
function bindItem(e, type) {
    var total = parseFloat(0);
    var str = "";
    var count = 1;
    $.each(e.item, function (i, item) {


        var subtot = parseFloat(item.ItemTotalAmount.toFixed(2));
        total += subtot;
        var itemnote = "";
        if (item.ItemNote != "") {
            itemnote = "<br /><small>" + item.ItemNote + "</small>";
        }
        var unit = (item.ItemUnit != null) ? item.ItemUnit : "";
        str += '<tr>';
        str += '<td>' + count + '</td>';
        str += '<td>' + item.ItemCode+" - "+ item.ItemName + " " + unit + itemnote + '</td>';
        // str += '<td>' + unit + '</td>';
        str += '<td>' + item.ItemQuantity + '</td>';
        str += '<td class="text-right">' + parseFloat(item.ItemUnitPrice).toFixed(2) + '</td>';
        str += '<td class="text-right">' + parseFloat(item.ItemSubTotal).toFixed(2) + '</td>';
        if (type == "sales") {
            str += '<td style="text-align:right;">' + (item.ItemTax) + "%" + '</td>';
        }
        str += '<td class="text-right">' + parseFloat(item.ItemTaxAmount).toFixed(2) + '</td>';
        str += '<td class="text-right">' + subtot.toFixed(2) + '</td>';
        str += '</tr>';
        count++;
    });
    return str;
}
// bind bill sundry
function bindSundry(e) {
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

function PrintInvoice(e, titlename, type) {
    $("#lblBillNo").text(e.summary.BillNo);
    $("#lblDate").text(convertToDate(e.summary.Date))
    $("#lblEmployee").text(e.summary.Cashier);
    if (e.summary.SExecutive == 0) {
        $("#executive").hide();
    }
    $("#lblpaytype").text(e.summary.paytype);

    $("#lblMC").text(e.summary.MC);

    if (e.summary.validityday != null) {
        $("#lblvalidity").text(e.summary.validityday + " Days");
    }

    //if (e.summary.DvNoteNo == "" || e.summary.DvNoteNo == null)
    //{
    //    $("#delvnote").hide();
    //}
    //else if (e.summary.DvNoteNo != "" || e.summary.DvNoteNo != null)
    //{
    //    $("#lbldelvnote").text(e.summary.DvNoteNo);
    //}

    if ((e.summary.DvNoteNo == "" || e.summary.DvNoteNo == null) && (e.summary.DvNo == "" || e.summary.DvNo == null)) {
        $("#delvnote").hide();
    }
    else if ((e.summary.DvNoteNo != "" || e.summary.DvNoteNo != null) && (e.summary.DvNo == "" || e.summary.DvNo == null)) {
        $("#lbldelvnote").text(e.summary.DvNoteNo);
    }
    else if ((e.summary.DvNoteNo == "" || e.summary.DvNoteNo == null) && (e.summary.DvNo != "" || e.summary.DvNo != null)) {
        $("#lbldelvnote").text(e.summary.DvNo);
    }
    else {
        $("#lbldelvnote").text(e.summary.DvNo + "," + e.summary.DvNoteNo);
    }

    if (type == "sales") {
        var lp = e.summary.PONo;

        if (lp == null || lp == "") {
            $(".lblpon").hide();
        }
        else {
            $("#lblPONo").text(lp);
        }
    }
    $("#lbltrn").text(e.summary.TRN);
    // bind Party details
    $("#lblParty").text(e.summary.PartyName);
    $("#lblAccNo").text(e.summary.PartyCode);

    //if (e.summary.DvNo != "" && e.summary.DvNo != null) {
    //    $("#dvnotediv").show();
    //    $("#lblDvNo").text(e.summary.DvNo);
    //} else {
    //    $("#dvnotediv").hide();
    //}


    remark = e.summary.TermsCondition.replace(/\n/g, "<br/>");
    var details = "";

    //Address
    if (e.summary.Address != null) {
        details += e.summary.Address;
    }

    //City
    //if (e.summary.City != null) {
    //    if (e.summary.Address != null) {
    //        details += ", " + e.summary.City;
    //    }
    //    else {
    //        details += e.summary.City;
    //    }
    //}

    //PO BOX 
    if (e.summary.Zip != null) {
        if (e.summary.City == null) {
            details += " PO BOX : " + e.summary.Zip;
        }
        else {
            details += "<br /> PO BOX : " + e.summary.Zip;
        }

    }


    //State && Country
    if (e.summary.State != null && e.summary.Country != null) {
        if (e.summary.Zip != null) {
            details += ", " + e.summary.State + ", " + e.summary.Country;
        }
        else if (e.summary.Zip == null) {
            details += e.summary.State + ", " + e.summary.Country;
        }
    }

    //Phone

    if (e.summary.Phone != null && e.summary.Mobile != null) {
        details += "<br/> Phone " + e.summary.Phone; // + ", " + e.summary.Mobile;
    }
        //else if (e.summary.Phone == null && e.summary.Mobile != null) {
        //    details += "<br/> Phone " + e.summary.Mobile;
        //}
    else if (e.summary.Phone != null && e.summary.Mobile == null) {
        details += "<br/> Phone " + e.summary.Phone;
    }

    //Fax

    if (e.summary.Fax != null) {
        if (e.summary.Phone != null && e.summary.Mobile != null) {
            details += ", Fax  : " + e.summary.Fax;
        }
        else if (e.summary.Phone == null && e.summary.Mobile != null) {
            details += ", Fax  : " + e.summary.Fax;
        }
        else if (e.summary.Phone != null && e.summary.Mobile == null) {
            details += ", Fax  : " + e.summary.Fax;
        }
        else {
            details += "<br/>Fax  : " + e.summary.Fax;
        }
    }


    //Email
    //if (e.summary.Email != null) {
    //    if (e.summary.Phone != null && e.summary.Mobile != null && e.summary.Fax != null) {
    //        details += "<br/> Email : " + e.summary.Email;
    //    }
    //    else if (e.summary.Phone != null && e.summary.Mobile != null && e.summary.Fax == null) {
    //        details += "<br/> Email : " + e.summary.Email;
    //    }
    //    else if (e.summary.Phone != null && e.summary.Mobile == null && e.summary.Fax != null) {
    //        details += "<br/> Email : " + e.summary.Email;
    //    }
    //    else if (e.summary.Phone == null && e.summary.Mobile != null && e.summary.Fax != null) {
    //        details += "<br/> Email : " + e.summary.Email;
    //    }
    //    else {
    //        details += " Email : " + e.summary.Email;
    //    }
    //}

    //TRN
    //if (e.summary.TRN != null) {
    //    if (e.summary.Email != null) {
    //        details += "<br/> TRN : " + e.summary.TRN;
    //    }
    //    else {
    //        details += " TRN : " + e.summary.TRN;
    //    }
    //}

    var SaleRemark = "";
    var Remarks = "";
    SaleRemark = e.summary.Remarks.replace(/\n/g, "<br/>");
    if (e.summary.Remarks != "") {
        Remarks += "<tr class='border-top'> <td colspan='8'> <strong><u>Remarks :</u></strong><br/>" + SaleRemark + "</td></tr>";
    }
    if (e.summary.Remarks == "") {
        Remarks += "";
    }

    $("[id$=lbladdress]").html(details);

    var str2 = "<td>Amount كمية</td><td class='text-right border-top'>" + parseFloat(e.summary.SubTotal).toFixed(2) + "</td></tr>";
    var count = 2;
    var str1 = "";
    var str3 = "";
    var st4 = "";

    // bind items
    var itemsData = bindItem(e, type);
    $('#itemtable').append(itemsData);
    var grt = parseFloat(e.summary.GrandTotal).toFixed(2);
    // bind total section
    var word = conNumber(grt);

    if (e.summary.Discount > 0) {
        //str2 += "<tr class='border-top'><td>Value Added Tax VAT<span style='direction:ltr'>(5.00%)</span> برميل</td><td  class='text-right'>" + parseFloat(e.summary.TaxAmount).toFixed(2) + "</td></tr>";
        str2 += "<tr class='border-top'><td>Discount خصم</td><td id='discountprint' class='text-right'>" + parseFloat(e.summary.Discount).toFixed(2) + "</td></tr> ";
        count++;
        str2 += "<tr class='border-top'><td>VAT<span style='direction:ltr'>(5.00%)</span> برميل</td><td  class='text-right'>" + parseFloat(e.summary.TaxAmount).toFixed(2) + "</td></tr>";
        //str2 += "<tr class='border-top'><td>Discount خصم</td><td id='discountprint' class='text-right'>" + parseFloat(e.summary.Discount).toFixed(2) + "</td></tr> ";
    }
    else {
        str2 += "<tr class='border-top'><td>VAT<span style='direction:ltr'>(5.00%)</span> برميل </td><td class='text-right'>" + parseFloat(e.summary.TaxAmount).toFixed(2) + "</td></tr>";
    }
    if (type != "nobillsundry") {
        // bind bill sundry
        str2 += bindSundry(e);
        if (e.billsundry.length > 0) {
            count += e.billsundry.length;
        }
    }

    // str2 += "<tr class='border-top'></tr>";

    var wordHtml = "<tr class='border-top'><td colspan='6'><strong>" + word + " Only</strong></td><th>Total المبلغ الإجمالي(AED)</th><th class='text-right'>" + grt + "</th></tr>";
    str3 += "<tr class='border-top'> <td colspan='8'> <strong><u>Terms And Conditions :</u></strong><br/>" + remark + "</td></tr>";
    var str4 = "<tr class='border-top'><td colspan='6' rowspan='" + count + "'></td>";
    if (e.summary.Transportation != null) {
        st4 += "<tr class='border-top'> <td colspan='4'> <strong>Driver : </strong>" + e.summary.Driver + "</td><td colspan='4'> <strong>Transportation : </strong>" + e.summary.Transportation + "</td></tr>";
    }
    else {
        st4 = "";
    }
    str1 = str4 + str2 + wordHtml + st4 + Remarks + str3;

    $('#itemtable1').append(str1);
    var originalpage = document.body.innerHTML;
    var printContent = $('#printit').html();
    $('body').html(printContent);

    // Naming Title

    var name = titlename;
    switch (name) {
        case 'PurchaseOrder':
            var titname = "Local Purchase Order - " + e.summary.PartyName + " - " + e.summary.BillNo;
            $('title').html(titname);
            break;
        case 'PurchaseReturn':
            var titname = "Purchase Return - " + e.summary.PartyName + " - " + e.summary.BillNo;
            $('title').html(titname);
            break;
        case 'PurchaseEntry':
            var titname = "Purchase Bill - " + e.summary.PartyName + " - " + e.summary.BillNo;
            $('title').html(titname);
            break;
        case 'DeliveryNote':
            var titname = "Delivery Note - " + e.summary.PartyName + " - " + e.summary.BillNo;
            $('title').html(titname);
            break;
        case 'ProForma':
            var titname = "Pro Forma Invoice - " + e.summary.PartyName + " - " + e.summary.BillNo;
            $('title').html(titname);
            break;
        case 'CreditSaleReturn':
            var titname = "Tax Return Note - " + e.summary.PartyName + " - " + e.summary.BillNo;
            $('title').html(titname);
            break;
        case 'CreditSale':
            var titname = "Tax Invoice - " + e.summary.PartyName + " - " + e.summary.BillNo;
            $('title').html(titname);
            break;
        default:
            var titname = "Tax Invoice - " + e.summary.PartyName + " - " + e.summary.BillNo;
            $('title').html(titname);
            break;
    }


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
    //  alert("Header - " + header + "\n items -" + items + "\n itemstable - "+itemstable+"\n terms-" + terms + "\n footer -" + footer + "\n full height - " + height);

    window.print();
}

function printiteminvoice(e) {
    var total = parseFloat(0);
    $.each(e.item, function (i, item) {
        var subtot = parseFloat(item.ItemTotalAmount.toFixed(2));
        total += subtot;
        var str = '<tr>';
        str += '<td>' + item.ItemCode + "-" + item.ItemName + " " + item.ItemUnit + '</td>';
        // str += '<td>' + item.ItemUnit + '</td>';
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
    $("#lblBillNo").text(e.summary.BillNo);
    $("#lblDate").text(convertToDate(e.summary.Date));
    $("#lblEmployee").text(e.summary.Cashier);
    $("#lblpaytype").text(e.summary.paytype);

    if (type == "sales") {
        $("#lblPONo").text(e.summary.PONo);
    }
    var details = e.summary.PartyName + "<br /> Acc No : " + e.summary.PartyCode;
    if (e.summary.Address != null) {
        details += "<br />" + e.summary.Address;
    }
        //if (e.summary.City != null) {
        //    details += "<br />" + e.summary.City;
        //}
    else if (e.summary.State != null) {
        details += "<br />" + e.summary.State;
    }
    else if (e.summary.Country != null) {
        details += "<br/>" + e.summary.Country;
    }
    else if (e.summary.Zip != null) {
        details += "<br /> PO BOX : " + e.summary.Zip;
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
    //if (e.summary.Email) {
    //    details += "<br/> Email : " + e.summary.Email
    //}
    if (e.summary.TRN) {
        details += "<br/> TRN : " + e.summary.TRN
    }
    $("#lblCustomer").html(details);
    var remark = e.summary.TermsCondition.replace(/\n/g, "<br/>");
    $("#lblTC").html(remark);
    // bind items
    var itemsData = bindPOSItem(e);
    $('#itemtable').append(itemsData);
    if (e.summary.Discount > 0) {
        $("#lblDiscAmt").text(parseFloat(e.summary.Discount).toFixed(2));
    }
    else {
        $(".discpt").hide();
    }
    $("#lblTax").text(parseFloat(e.summary.TaxAmount).toFixed(2));
    $("#lblAmount").text(parseFloat(e.summary.SubTotal).toFixed(2));
    var str = "";
    $.each(e.billsundry, function (i, billsundry) {
        str += "<tr class='tabletitle'>";
        str += '<td colspan="3" class="Rate"><h2><strong>' + billsundry.BillSundry + '</strong></h2></td>';
        str += '<td class="rate"><h2>' + parseFloat(billsundry.BsAmount).toFixed(2) + '</h2></td>';
        str += '</tr>';
    });
    str += '<tr class="tabletitle"><td colspan="3" class="Rate"><h2><strong>Grand Total المبلغ الإجمالي(aed)</strong></h2></td>' +
	'<td class="rate"><h2>' + parseFloat(e.summary.GrandTotal).toFixed(2) + '</h2></td></tr>';
    $('#posfoot').append(str);
    var originalpage = document.body.innerHTML;
    var printContent = $('#printit').html();
    $('body').html(printContent);
    var titlename = "Tax Invoice - " + e.summary.PartyName + " - " + e.summary.BillNo;
    $('title').html(titlename);

    window.print();
}
//itembind for POS
function bindPOSItem(e) {
    var total = parseFloat(0);
    var str = "";
    var count = 1;
    $.each(e.item, function (i, item) {
        var subtot = parseFloat(item.ItemTotalAmount.toFixed(2));
        total += subtot;
        var itemnote = "";
        if (item.ItemNote != "") {
            itemnote = "<br /><small>" + item.ItemNote + "</small>";
        }
        var unit = (item.ItemUnit != null) ? item.ItemUnit : "";
        str += '<tr>';
        str += '<td>' + item.ItemName + ' ' + unit + itemnote + '</td>';
        str += '<td>' + item.ItemQuantity + '</td>';
        str += '<td class="text-right">' + parseFloat(item.ItemUnitPrice).toFixed(2) + '</td>';
        str += '<td class="text-right">' + parseFloat(item.ItemSubTotal).toFixed(2) + '</td>';
        //str += '<td class="text-right">' + parseFloat(item.ItemTaxAmount).toFixed(2) + '</td>';
        //str += '<td class="text-right">' + subtot.toFixed(2) + '</td>';
        str += '</tr>';
        count++;
    });
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

});
$('.salesexec').select2({
    dropdownParent: $('#modal-create')
});



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
//---------------------------------------------------

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

        $(function () {
            $(".textareapopup").wysihtml5();
        });

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