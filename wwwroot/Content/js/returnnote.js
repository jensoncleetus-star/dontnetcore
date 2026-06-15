var count = 1, type = '';
limits = 500;
//Add Row
function addrow(t, action, ItemUnit, ItemQuantity, RetItemQuantity, DvItemQuantity, ItemBalance, Item, ItemCode, ItemName, ItemWithCode, ItemNote, itemdata, ItemDiscount, RecItemQuantity, DamItemQuantity, MissItemQuantity, BaseQty,invoice) {
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
        var htdata = "";
        var itemnote = "";
        var notbtn = "";
        var divid = "item_name_" + Item;
        var hiretype = "";
        var selitem = "";
        var selunit= "";
        var invoice = "";

        tab1 = tabindex + 1;
        tab2 = tabindex + 2;
        tab3 = tabindex + 3;
        tab4 = tabindex + 4;
        tab5 = tabindex + 5;
        tab6 = tabindex + 6;
        tab7 = tabindex + 7;
        if (action != '') {
            type = action;
        }

        Option = "<option value='" + Item + "'>" + ItemWithCode + "</option>";
        if (Item != null) {
            row = "<tr class='item_" + Item + "' id='item_" + count + "'>";
            selitem = "<select class='form-control' " + required + " data-id='" + count + "' placeholder='Item Name' id='item_name_" + count + "'  data-msg-required='The Item Name is required' onchange='GetItemdetails(this," + count + ",\"" + type + "\")'>" + Option + "</select>";
        } else {
            Option = (typeof Item == 'undefined') ? "<option></option>" : Option;
            selitem = "<select class='form-control item_name' " + required + " data-id='" + count + "' placeholder='Item Name' id='item_name_" + count + "'  data-msg-required='The Item Name is required' onchange='GetItemdetails(this," + count + ",\"" + type + "\")'>" + Option + "</select>";
        }


        if (count == 1) {
            required = 'required="required"';
        }
        var inote = "";
        var readonly = "";
        var readonlyqty = "";
        var deletebtn = "";
        var disablebtn = "";
        if (itemdata) {
            inote = itemdata.note;
            hiretype = itemdata.hiretype;

            if ((itemdata != null && itemdata.bundle != null && itemdata.bundle.length > 0)) {
                readonly = "readonly='readonly'"
            }
            if (itemdata.note == "-:{Bundle_Item}") {
                readonlyqty = "readonly='readonly'"
                disablebtn = "disabled='disabled'";
            } else {
                deletebtn = "<button tabindex='" + tab7 + "' style='text-align: right;' class='btn btn-danger' type='button' value='Delete' onclick='deleteRow(this," + Item + ")'><i class='fa fa-trash fa-1x'></i></button> ";
            }

        }
        if(action != "edit")
        ItemBalance = ItemBalance > 0 ? (ItemBalance - ItemQuantity) : 0;

        itemnote = '<div id="modal-item-' + count + '" class="modal fade" role="dialog" aria-hidden="true"><div class="modal-dialog"><div class="modal-content">' +
            '<div class="form-group"><textarea name="itemnote" cols="40" rows="10" class="form-control itemnote" id="itemnote-' + count + '">' + inote + '</textarea></div>' +
            '<div class="form-group"><button class="btn btn-info" type="button" data-dismiss="modal">Save</button></div>' +
            '</div></div></div>';
        notbtn = "<button type='button' " + disablebtn + " class='itnote btn btn-default btn-flat' data-toggle='modal' data-target='#modal-item-" + count + "'><i class='fa fa-1x fa-file-text-o'></i></button>";

        //  var itemaddbtn = "<span class='input-group-btn'><a type='button' href='/Item/AddItem' class='modal-create-lg btn btn-success btn-flat'><i class='fa fa-1x fa-plus-circle'></i></a>" + notbtn + "</span>";
        var itemaddbtn = "<span class='input-group-btn'>" + notbtn + "</span>";

        data = "<td class='text-center' id=" + divid + "> " + slno + " </td>" +
                "<td class='input-group input-group-sm'> " + selitem + itemaddbtn + "</td>" +
                "<td style='width:100px;'><select class='form-control units unit_name_" + count + "' id='unit_name_" + count + "' data-id='" + count + "' onchange='unitchange(this," + count + ",\"" + type + "\");'></select></td>" +
                "<td> <input type='number' name='product_quantity[]' onchange='quantity_change(" + count + ");' id='product_quantity_" + count + "' value='" + parseFloat(ItemQuantity).toFixed(2) + "'  class='total_qntt_" + count + " form-control text-right quty' placeholder='0' min='0' tabindex='" + tab2 + "' " + readonlyqty + " /></td>" +

                "<td class='chide'><input type='number' name='product_received[]' onchange='received_qty_change(" + count + ");' id='received_quantity_" + count + "' value='" + parseFloat(RecItemQuantity).toFixed(2) + "'  class='total_receive_qntt_" + count + " form-control text-right recquty' placeholder='0' min='0' tabindex='" + tab2 + "' readonly='readonly' /></td>" +
                "<td class='chide'><input type='number' name='product_damage[]' onchange='damAndmiss_qty_change(" + count + ");' id='damage_quantity_" + count + "' value='" + parseFloat(DamItemQuantity).toFixed(2) + "'  class='total_damage_qntt_" + count + " form-control text-right damquty' placeholder='0' min='0' tabindex='" + tab2 + "' " + readonly + " /></td>" +
                "<td class='chide'><input type='number' name='product_missing[]' onchange='damAndmiss_qty_change(" + count + ");' id='missing_quantity_" + count + "' value='" + parseFloat(MissItemQuantity).toFixed(2) + "'  class='total_missing_qntt_" + count + " form-control text-right missquty' placeholder='0' min='0' tabindex='" + tab2 + "' " + readonly + " /></td>" +

                "<td class='chide'><input type='number' name='ret_product_quantity[]' onchange='ret_quantity_change(" + count + ");' id='ret_total_qntt_" + count + "' value='" + parseFloat(RetItemQuantity).toFixed(2) + "'  class='ret_total_qntt_" + count + " form-control text-right retquty' placeholder='0' min='0' tabindex='" + tab2 + "' readonly='readonly'/></td>" +
                "<td><input type='number' name='dv_product_quantity[]' onchange='dv_quantity_change(" + count + ");' id='dv_total_qntt_" + count + "' value='" + parseFloat(DvItemQuantity).toFixed(2) + "'  class='dv_total_qntt_" + count + " form-control text-right dvquty' placeholder='0' min='0' tabindex='" + tab2 + "' readonly='readonly'/></td>" +
                "<td><input type='number' name='item_balance[]' id='item_balance_" + count + "' value='" + parseFloat(ItemBalance).toFixed(2) + "' class='item_balance_" + count + " form-control text-right item_balance' placeholder='0.00' min='0' tabindex='" + tab5 + "' readonly='readonly'/></td>" +
                "<td class='text-center' id='buttonid'> " + deletebtn + itemnote + htdata +
                "<input type='hidden' class='hire_type' name='hire_type[]' id='hire_type_" + count + " value='" + hiretype + "'/> " +
                "<input type='hidden' class='item_disc_" + count + " itemdisc' id='item_disc_" + count + "' value='" + ItemDiscount + "'/>" +
                "<input type='hidden' class='item_id_" + count + " itemid' id='item_id_" + count + "' value='" + Item + "'/>"+
                "<input type='hidden' class='baseqty_" + count + " baseqty' id='baseqty_" + count + "' value='" + BaseQty + "'/>"+
                "<input type='hidden' class='invoice_" + count + " invoice' id='invoice_" + count + "' value='" + invoice + "'/></td>";
        row += data + "</tr>";
        $('#' + t).append(row);
        searchItem();

        if (itemdata && itemdata.note != "-:{Bundle_Item}") {
            createUnitList(itemdata, count);
        }
        else if (typeof itemdata !== "undefined" && itemdata.note == "-:{Bundle_Item}") {
            createBundleUnitList(itemdata, count);
        }
        
        //----------------------------

        if (action == "edit") {
            $("#actionvalue").val(action);
        } else {
            $("#actionvalue").val("");
        }


        HireItemTotal();
        count++;
        setTabIndex();

    }
}

function createBundleUnitList(result, dataid) {
    // clear previous content
    //alert(result.ItemUnit);
    $('#unit_name_' + dataid).empty();
    if (result.ItemUnit != null) {
        var newOption = $('<option></option>');
        newOption.val(result.ItemUnitID).html(result.ItemUnit);
        $('#unit_name_' + dataid).append(newOption);
    }
    else {
        $('#unit_name_' +count).append('<option></option>');
    }
}
//item details
function GetItemdetails(selectObject, dataid, action) {

    if (selectObject.value) {
        var ItemId = selectObject.value;
        var flag = true;
        if (ItemId != null) {
            $(".item_" + ItemId).each(function () {
                var rowid = $(this).attr("id");
                var subitem = $("#" + rowid + " .itemdisc").val();
                if (subitem == 0) {
                    flag = false;
                }
            });
            if (flag == false) {
                alert("Sorry You Cant Add An Item More Than One Time");
                $(selectObject).val(null).trigger('change');
            }
            else {
                itemUpdate(selectObject, dataid, action);
            }
        }
    }

}
function checkItemBalance(arg) {
    var newqty = 0;
    var itembal = 0;
    $("#addinvoiceItem tr").each(function () {
        var qty = $(this).find('.quty').val();
        if (qty > 0) {
            var dvquty = $(this).find('.dvquty').val();
            var item_balance = $(this).find('.item_balance').val();
            newqty = parseFloat(newqty) + parseFloat(qty);
            itembal = item_balance - newqty;
            //alert(itembal);
            $(this).find('.item_balance').val(itembal.toFixed(2))
        }
    });
}

// update item details
function itemUpdate(selectObject, dataid, action) {
    var sentry = "";

    var url = "";
    if (action == "edit") {
        url = '/HireReturn/GetHrItems';
        sentry = getQueryString('');
    } else {
        url = '/HireReturn/SearchHireItemById';
        sentry = $("#ddlInvoice").val();
    }

    $.ajax({
        url: url,
        dataType: 'json',
        data: { ItemId: selectObject.value, entryId: sentry },
        cache: true,
        success: function (data) {
            if (data != "") {
                var qtySum = 0;
                $.each(data, function (i, item) {
                    var RetItemQuantity = (item.RetItemQuantity != null) ? item.RetItemQuantity : 0;
                    var DvItemQuantity = (item.DvItemQuantity != null) ? item.DvItemQuantity : 0;
                    var itmbal = (parseFloat(DvItemQuantity).toFixed(2) - parseFloat(RetItemQuantity)).toFixed(2);
                    qtySum += parseFloat(itmbal);
                });
                if (qtySum > 0 || action == "edit") {
                    $.each(data, function (i, item) {
                        var chckv = 0;
                        var RetItemQuantity = (item.RetItemQuantity != null) ? item.RetItemQuantity : 0;
                        var DvItemQuantity = (item.DvItemQuantity != null) ? item.DvItemQuantity : 0;
                        var itmbalance = (parseFloat(DvItemQuantity) - parseFloat(RetItemQuantity)).toFixed(2);
                        //$("#addinvoiceItem tr.item_" + item.Item).each(function () {
                        //    var itunit = $(this).find('.units').val();
                        //    if (itunit == item.ItemUnit) {
                        //        chckv = 1;
                        //    }
                        //});

                        if ((chckv == 0 && itmbalance > 0) || action == "edit") {

                            var tbody = $("#normalinvoice tbody");
                            if (tbody.children().length > 0) {
                                tbody.children("tr").each(function () {
                                    var rowid = $(this).attr("id");
                                    var itemid = $("#" + rowid + " .itemid").val();
                                    if (itemid == 'undefined') {
                                        $(this).closest("tr").remove();
                                    }
                                });
                            }

                            if (action == "edit") {
                                RetItemQuantity = RetItemQuantity - item.ItemQuantity;
                                addrow('addinvoiceItem', '', item.ItemUnit, item.ItemQuantity, RetItemQuantity, DvItemQuantity, itmbalance, item.Item, item.ItemCode, item.ItemName, item.ItemWithCode, item.ItemNote, item, item.ItemDiscount, item.ReceivedQty, item.DamageQty, item.MissingQty, item.BaseQty);
                            } else {
                                addrow('addinvoiceItem', '', item.ItemUnit, itmbalance, RetItemQuantity, DvItemQuantity, itmbalance, item.Item, item.ItemCode, item.ItemName, item.ItemWithCode, item.ItemNote, item, item.ItemDiscount, itmbalance, 0, 0);
                            }

                            var qtySumBundle = 0;
                            $.each(item.bundle, function (i, item) {
                                var RetItemQuantitys = (item.RetItemQuantity != null) ? item.RetItemQuantity : 0;
                                var DvItemQuantitys = (item.DvItemQuantity != null) ? item.DvItemQuantity : 0;
                                var itmbals = (parseFloat(DvItemQuantity).toFixed(2) - parseFloat(RetItemQuantity)).toFixed(2);
                                qtySumBundle += parseFloat(itmbals);
                            });

                            if (qtySumBundle > 0 || action == "edit") {
                                $.each(item.bundle, function (i, item) {
                                    var chckvs = 0;
                                    var RetItemQuantitys = (item.RetItemQuantity != null) ? item.RetItemQuantity : 0;
                                    var DvItemQuantitys = (item.DvItemQuantity != null) ? item.DvItemQuantity : 0;
                                    var itmbalances = (parseFloat(DvItemQuantity * item.BaseQty) - parseFloat(RetItemQuantity)).toFixed(2);

                                    $("#addinvoiceItem tr.item_" + item.Item).each(function () {
                                        var itunit = $(this).find('.units').val();
                                        if (itunit == item.ItemUnit) {
                                            chckvs = 1;
                                        }
                                    });

                                    if ((chckvs == 0 && itmbalances > 0) || action == "edit") {

                                        if (action == "edit") {
                                            RetItemQuantitys = RetItemQuantitys - item.ItemQuantity;
                                            addrow('addinvoiceItem', '', item.ItemUnit, item.ItemQuantity, RetItemQuantitys, DvItemQuantitys, itmbalances, item.Item, item.ItemCode, item.ItemName, item.ItemWithCode, item.ItemNote, item, item.ItemDiscount, item.ReceivedQty, item.DamageQty, item.MissingQty, item.BaseQty);

                                        } else {
                                            addrow('addinvoiceItem', '', item.ItemUnit, item.ItemQuantity, RetItemQuantity, DvItemQuantity * item.BaseQty, itmbalances, item.Item, item.ItemCode, item.ItemName, item.ItemWithCode, item.ItemNote, item, item.ItemDiscount, item.ItemQuantity, 0, 0, item.BaseQty);
                                        }
                                    }
                                });
                            }
                        }
                        rowSubTotal(item.Item);
                        CalculatetblItemListSum();
                        HireItemTotal();
                    });
                } else {
                    $('#ReturnTable').hide();
                    alert("No Items Pending !!");
                }
                addrow('addinvoiceItem', '', '', '0.00', '0.00', '0');

            } else {
                addrow('addinvoiceItem', '', '', '0.00', '0.00', '0');
            }
        }
    });
}
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
            stock = stock - (qty - quantity);
            minstock = minstock / confactor;
            stock = stock / confactor;
            var tostock = stock - quantity;
            var totstock = tostock / confactor;

            //var totstock = stock - qty;
            if (totstock <= minstock && totstock >= 0) {
                alert("Stock Exceeds Minimum Stock");
            }
            if (totstock < 0) {
                stock = stock.toFixed(2);
                alert("This Item Is Going To Out of Stock!!! Only " + stock + " " + unitname + "Items Are Available In Stock..");
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
    }
}
// create units based on primary and secondary
function createUnitList(result, dataid) {
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
    var saleEntryid = $("#ddlInvoice").val();
    var dvid = getQueryString('');


    $(".item_name").select2({
        placeholder: 'Search Item by Name or Code',
        minimumInputLength: 0,
        ajax: {
            url: "/HireReturn/SearchHireItem",
            dataType: 'json',
            type: "POST",
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    sentry: saleEntryid,
                    qrystr: dvid
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


function quantity_change(arg) {
    var qty = $(".total_qntt_" + arg).val() || 0;
    var itemid = $(".item_id_" + arg).val();
    var actqty = $(".baseqty_" + arg).val();

    var retquantity = $(".ret_total_qntt_" + arg).val() || 0;
    var dvquantity = $(".dv_total_qntt_" + arg).val() || 0;
    var totqty = parseFloat(retquantity) + parseFloat(qty);
    var itembalance = parseFloat(dvquantity) - parseFloat(totqty);

    if (itembalance >= 0) {
        $(".total_receive_qntt_" + arg).val(parseFloat(qty).toFixed(2));

        var tbody = $("#normalinvoice tbody");
        if (tbody.children().length > 0) {
            tbody.children("tr").each(function () {
                var rowid = $(this).attr("id");
                var subitem = $("#" + rowid + " .itemdisc").val();
                var baseqty = $("#" + rowid + " .baseqty").val();

                //alert(subitem +","+ itemid);

                var totalqty = qty * baseqty;

                if (subitem == itemid) {
                    $("#" + rowid + " .quty").val(totalqty.toFixed(2));
                    $("#" + rowid + " .recquty").val(totalqty.toFixed(2));
                }

                var retqty = $("#" + rowid + " .retquty").val();
                var dvqty = $("#" + rowid + " .dvquty").val();
                var quty = $("#" + rowid + " .quty").val();
                var finalqty = parseFloat(retqty) + parseFloat(quty);
                var itembal = parseFloat(dvqty) - parseFloat(finalqty);

                $("#" + rowid + " .item_balance").val(itembal.toFixed(2));
            });
        }
    } 

    minstockcheck(arg);
    rowSubTotal(arg);
    CalculatetblItemListSum();
}

function damAndmiss_qty_change(arg) {
    var qty = parseFloat($(".total_qntt_" + arg).val());
    var damageqty = parseFloat($(".total_damage_qntt_" + arg).val());
    var missqty = parseFloat($(".total_missing_qntt_" + arg).val());

    var sumqty = damageqty + missqty;
    if ((qty < sumqty) || (qty < damageqty)) {
        alert("Damage Quantity / Missing Quantity  Should Less than Quantity....!!");
        $(".total_damage_qntt_" + arg).val(0.00);
    } else {
        var recqty = qty - (damageqty + missqty);
        $(".total_receive_qntt_" + arg).val(recqty.toFixed(2));
    }
}


function rowSubTotal(arg) {

    var quantity = $(".total_qntt_" + arg).val() || 0;
    var retquantity = $(".ret_total_qntt_" + arg).val() || 0;
    var dvquantity = $(".dv_total_qntt_" + arg).val() || 0;
    var totqty = parseFloat(retquantity) + parseFloat(quantity);
    var itembalance = parseFloat(dvquantity) - parseFloat(totqty);

    // ------item balance when -ve-----------
    if (itembalance < 0) {
        var action = $("#actionvalue").val();
        alert("Invalid Item Balance..!!");
        var newqty = quantity - dvquantity;
        var newqty1 = quantity - newqty;
        if (action == "edit") {

            $(".total_qntt_" + arg).val(quantity);
            newqty1 = parseFloat(dvquantity) - parseFloat(quantity);
            $(".item_balance_" + arg).val(newqty1.toFixed(2));
        } else {
            $(".total_qntt_" + arg).val(newqty1);
            newqty1 = parseFloat(dvquantity) - parseFloat(newqty1);
            $(".item_balance_" + arg).val(newqty1.toFixed(2));
        }
        quantity_change(arg);
    } else {
        $(".item_balance_" + arg).val(itembalance.toFixed(2));
    }

}

function CalculatetblItemListSum() {
    var qty = $(".quty").val();
    var tbody = $("#normalinvoice tbody");
    if (tbody.children().length > 0) {
        var gtQty = 0;
        var recQty = 0;
        var damQty = 0;
        var misQty = 0;

        $(".quty").each(function () {
            var subQty = this.value;
            subQty = subQty || 0;
            gtQty = parseFloat(gtQty) + parseFloat(subQty);
        });

        $(".recquty").each(function () {
            var subQty = this.value;
            subQty = subQty || 0;
            recQty = parseFloat(recQty) + parseFloat(subQty);
        });

        $(".damquty").each(function () {
            var subQty = this.value;
            subQty = subQty || 0;
            damQty = parseFloat(damQty) + parseFloat(subQty);
        });

        $(".missquty").each(function () {
            var subQty = this.value;
            subQty = subQty || 0;
            misQty = parseFloat(misQty) + parseFloat(subQty);
        });



        $("[id$=ItemCount]").val(tbody.children().length);
        $("[id$=ItemQty]").text((gtQty).toFixed(2));

        $("[id$=recQty]").text((recQty).toFixed(2));
        $("[id$=damQty]").text((damQty).toFixed(2));
        $("[id$=misQty]").text((misQty).toFixed(2));

    }
}
//item unit change
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
        //var cfactor = parseFloat($('#cfactor_' + arg).val());
        var price = parseFloat($("#base_rate_" + arg).attr("data-value"));
        //var newprice = parseFloat(price * cfactor);
        $(".price_item_" + arg).val(price.toFixed(2));
    }
    rowSubTotal(arg);
    CalculatetblItemListSum();
}

//Delete a row of table
function deleteRow(t,item) {
    var classname = $(t).closest('tr').attr('class');
    if (classname == 'item_') alert("Sorry you can't delete this row.");
    else {
        var r = confirm("Are you sure you want to delete this..?");
        if (r == true) {
            var e = t.parentNode.parentNode;
            e.parentNode.removeChild(e);

            var tbody = $("#normalinvoice tbody");
            if (tbody.children().length > 0) {
                tbody.children("tr").each(function () {
                    var rowid = $(this).attr("id");
                    var subitem = $("#" + rowid + " .itemdisc").val();
                    var index = $("#" + rowid + " .itemdisc").index(this);
                    if (subitem == item) {
                        $(this).closest("tr").remove();
                    }
                });
            }
        }
    }
    CalculatetblItemListSum();
    var i = 1;
    $('#addinvoiceItem tr').each(function () {
        $(this).find('td:first').text(i);
        i++;
    });
}

// calculate total value
function HireItemTotal() {
    var qty = $(".quty").val();
    var itemcount = $("#normalinvoice tbody").children().length - 1;
    $("#ItemCount").val(itemcount);
    if (qty != 0) {
        var tbody = $("#normalinvoice tbody");
        if (tbody.children().length > 0) {
            var gtQty = 0;
            $(".quty").each(function () {
                var subQty = this.value;
                gtQty = parseFloat(gtQty) + parseFloat(subQty);
            });

            $("[id$=ItemCount]").val(tbody.children().length);
            $("[id$=ItemQty]").text((gtQty).toFixed(2));

        }
    }
}

function hiretypechange() {
    var hiretype = $("#ddlHireType").val();
    var hirerate = "";
    if (hiretype != null) {
        var tbody = $("#normalinvoice tbody");
        if (tbody.children().length > 0) {
            tbody.children("tr").each(function () {
                var rowid = $(this).attr("id");
                var item = $("#" + rowid + " .item_name").val();
                if (item != null) {
                    $.ajax({
                        url: '/HireType/GetHireRatebyTypeAndId',
                        type: "GET",
                        dataType: "JSON",
                        data: { hiretype: hiretype, item: item },
                        success: function (result) {
                            if (count != (tbody.children().length - 1)) {
                                $("#" + rowid + " .hirerate").val(result.toFixed(2));
                                count++;
                            }
                            var arr = rowid.split('_');
                            var arg = arr[1];
                            rowSubTotal(arg);
                            CalculatetblItemListSum();
                            HireItemTotal();
                        }
                    });
                }

            });
        }
    }
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
    $("#PoNo").hide();
    function ItemsBind(ritem, rtype, bcount) {
        var itSubtotal = parseFloat(ritem.ItemSubTotal);
        var itDiscount = parseFloat(ritem.ItemDiscount);
        var itTaxable = itSubtotal - itDiscount;
        var TaxableAmount = (Layout != "Scaffold") ? parseFloat(ritem.ItemSubTotal).toFixed(2) : itTaxable.toFixed(2);
        TotTaxAmount += rtype != "bundle" ? ritem.ItemTaxAmount : 0;
        TotTaxableAmount += rtype != "bundle" ? TaxableAmount : 0;
        GrandTot += rtype != "bundle" ? ritem.ItemTotalAmount : 0;
        QtyTot += (rtype != "bundle" && ritem.KeepStock) ? ritem.ItemQuantity : 0;
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
            dvField1 += '<td class="text-right"><b>' + TaxableAmount + '</b></td>';
            dvField2 += '<td class="text-right"><b>' + parseFloat(ritem.ItemTaxAmount).toFixed(2) + '</b></td>';
            dvField2 += '<td class="text-right"><b>' + parseFloat(ritem.ItemTotalAmount).toFixed(2) + '</b></td>';
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
            var CBM = (ritem.CBM != null && ritem.CBM != "") ? (parseFloat(ritem.CBM) * parseFloat(ritem.ItemQuantity)).toFixed(2) : "";
            var Weight = (ritem.Weight != null && ritem.Weight != "") ? (parseFloat(ritem.Weight) * parseFloat(ritem.ItemQuantity)).toFixed(2) : "";
            var img = "";
            wgt = parseFloat(wgt) + parseFloat(Weight || 0);
            cbm = parseFloat(cbm) + parseFloat(CBM || 0);
            if (ritem.img != null && ritem.img.length > 0) {
                $.each(ritem.img, function (j, imgs) {
                    var im = "/uploads/itemimages/" + ritem.Id + "/thumb_" + imgs.FileName;
                    img = "<img width='68' height='46' src='/uploads/itemimages/" + ritem.Id + "/thumb_" + imgs.FileName + "'/>";
                });
            }
            var itnamecols = (Weight == "") ? ((CBM == "") ? 3 : 2) : 1;
            if (img == "") {
                itnamecols++;
            }
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
            //Row += dvField1;
            
            //Row += dvField2;


            var RetItemQuantitys = (ritem.RetItemQuantity != null) ? ritem.RetItemQuantity : 0;
            var DvItemQuantitys = (ritem.DvItemQuantity != null) ? ritem.DvItemQuantity : 0;
            var itmbalances = (parseFloat(DvItemQuantitys) - parseFloat(RetItemQuantitys)).toFixed(2);
           
            Row += '<td><b>' + ritem.Received.toFixed(2) + '</b></td>';
            Row += '<td><b>' + ritem.Damage.toFixed(2) + '</b></td>';
            Row += '<td><b>' + ritem.Missing.toFixed(2) + '</b></td>';
            Row += '<td><b>' + parseFloat(itmbalances).toFixed(2) + '</b></td>';
        }
        Row += '</tr>';
        return Row;
    }
    $.each(e.item, function (i, item) {
        qty += item.ItemQuantity;
        
        str += ItemsBind(item);
        count++;
        if (item.bundle != null && item.bundle.length > 0) {
            $.each(item.bundle, function (j, itemss) {
                var bcount = j + 1
                str += ItemsBind(itemss, "bundle", bcount);
            });
        }
    });
    if (Layout == "Scaffold") {
        var weihtv = (parseFloat(wgt) != 0) ? parseFloat(wgt).toFixed(2) : "";
        var cbmv = (parseFloat(cbm) != 0) ? parseFloat(cbm).toFixed(2) : "";
        str += "<tr class='border-top'><td colspan='3' class='text-right'><b>TOTAL</b></td><td class='text-center'><b>" + weihtv + "</b></td><td class='text-center'><b>" + cbmv + "</b></td><td></td><td></td><td></td><td></td><td></td></tr>";
    }
    return str;
}

function PrintInvoice(e, type, dvitem, conType) {
    //alert(conType);

    var Layout = (typeof e.layout == 'undefined') ? "Default" : e.layout.Name;
    var Bill_Total = $("#Bill_Total").html();
    var Bill_Tax = $("#Bill_Tax").html();
    var Bill_Discount = $("#Bill_Discount").html();
    var Terms = $("#Terms").html();
    var Bill_Amount = $("#Bill_Amount").html();
    if (e.summary.ComHeadCheck == 0) {

        $("#ComHeadCheck").hide();
    }
    else {
        $("#ComHeadCheck").show();
    }

    if (typeof Bill_Total === 'undefined') {
        Bill_Total = "Total  المبلغ الإجمالي<span style='direction:ltr'>(AED)</span>";
    }
    if (typeof Bill_Tax === 'undefined') {
        Bill_Tax = "VAT(5.00%) الضريبة ";
    }
    if (typeof Bill_Discount === 'undefined') {
        Bill_Discount = "Discount خصم ";
    }
    if (typeof Terms === 'undefined') {
        Terms = "Terms And Conditions";
    }
    if (typeof Bill_Amount === 'undefined') {
        Bill_Amount = "Amount كمية ";
    }
    if (e.summary.ProCheck == 0 && (e.summary.PrjNameCode != null && e.summary.PrjNameCode != "")) {
        $("#lblProject").text(e.summary.PrjNameCode);
    }
    else {
        $("#divproject").hide();
    }

    $("#lblBillNo").text(e.summary.BillNo);
    $("#lblDate").text(convertToDate(e.summary.Date));
    $("#lblpaytype").text(e.summary.paytype);

    if (e.summary.LPONo != null) {
        $("#lblLPONo").text(e.summary.LPONo);
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



    var tc = (e.summary.Note!=null)?e.summary.Note.replace(/\n/g, "<br/>"):"";
    var remark = (e.summary.Remarks!=null)?e.summary.Remarks.replace(/\n/g, "<br/>"):"";
    // bind Party details
    if (Layout == "Scaffold") {
        var Caddres = (e.summary.Address != null) ? e.summary.Address : '';
        var Cperson = (e.summary.ContactPerson != null) ? e.summary.ContactPerson : '';
        var CMobile = (e.summary.Mobile != null) ? e.summary.Mobile : '';
        var CEmail = (e.summary.Email != null) ? e.summary.Email : '';
        var CTRN = (e.summary.TRN != null) ? e.summary.TRN : '';
        var cDetais = "<tr><th style='width:28% !important;'>CUSTOMER NAME</th><td>:</td><td width='69%'>" + e.summary.PartyName + "</td><tr>" +
                        "<tr><th style='width:28% !important;'>ADDRESS</th><td>:</td><td>" + Caddres + "</td></tr>" +
                        "<tr><th style='width:28% !important;'>CONTACT PERSON</th><td>:</td><td>" + Cperson + "</td></tr>" +
                        "<tr><th style='width:28% !important;'>MOBILE NO</th><td>:</td><td>" + CMobile + "</td></tr>";
        cDetais += CEmail != '' ? "<tr><th style='width:28% !important;'>EMAIL</th><td>:</td><td>" + CEmail + "</td></tr>" : "";
        cDetais += "<tr><th style='width:28% !important;'>TRN</th><td>:</td><td>" + CTRN + "</td></tr>";

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

    if (Layout == "Default") {
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
        //str1 = wordHtml + str3 + str2 + remarks;

    }
    else if (Layout == "Scaffold") {
      
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
            var startDate = moment(From, "DD.MM.YYYY");
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


            //console.log(diff);
            Subject += '<b>HIRE OF ALUMINIUM SCAFFOLDING FOR ' + diff + ' ' + HtypeV + '(STARTING FROM ' + From + ' TO ' + To + ' ) </b>';
        } else {
            $('#HSubject').hide();
        }
        $('#Subject').append(Subject);
        if (tc != "") {
            var Terms_C = "<tr style='border:1px solid;'><td><p style='text-align:left;text-decoration: underline;font-weight: 800;margin-bottom: 5px;'>Terms & Conditions</p><div style='padding-left:5px;'>" + tc + "</div></td></tr>";
            $('#termstable').append(Terms_C);
            $('#termstable').removeClass("hidden");
        }

    }
    else {
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


    if ($('#hideheader').prop('checked') == true) {
        $('#ComHeadCheck').hide();
        $('#ComfootCheck').hide();
        //$(".invoice.print").css("margin-top", "100px");
        $("#comHeader").css("margin-top", "100px");
    }
    else {
        $('#ComHeadCheck').show();
        $('#ComfootCheck').show();
    }

    $('#itemtable1').append(str1);
    var originalpage = document.body.innerHTML;
    var printContent = $('#printit').html();
    $('body').html(printContent);


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
        case 'HireReturn':
            var titname = "HireReturn - " + e.summary.PartyName + " - " + e.summary.BillNo;
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

    // console.log("cusHeight - " + cusHeight + "inHeight - " + inHeight + "Header - " + header + "\n items -" + items + "\n itemstable - " + itemstable + "\n terms-" + terms + "\n footer -" + footer + "\n full height - " + height);

    setTimeout(function () { window.print(); }, e.summary.TimeOut);
}


function fieldReset() {
    $("#ItemQty").text(0.00);
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
    });
    //sec unit change
    $(document).on('change', '#SubUnitId', function (event) {

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

function BindInvoices(cust) {
    $('#ddlInvoice').val(null).trigger('change');
    var project = $('#ddlProject').val();
    //bind to salesentry
    $("#ddlInvoice").select2({
        placeholder: 'Search Hire Invoices ',
        minimumInputLength: 0,
        ajax: {
            url: "/CreditSale/SearchHireEntry",
            dataType: 'json',
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    cust: cust,
                    project:project
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

function BindDataInvoice(invoice) {
    if (invoice != null) {
        $.ajax({
            url: '/HireReturn/GetHireType',
            type: "POST",
            dataType: "JSON",
            data: { Invoice: invoice },
            success: function (result) {
                $("#ddlHType").val(result.Hty);
                if (result.Hty != null) {
                    $('#ReturnTable').show();
                    $.ajax({
                        url: '/CreditSale/GetSEItemsHire',
                        dataType: 'json',
                        data: { SalesEntryID: invoice },
                        cache: true,
                        success: function (data) {
                            if (data != "") {
                                var qtySum = 0;
                                $('#addinvoiceItem').html('');
                                $.each(data, function (i, item) {
                                    var RetItemQuantity = (item.RetItemQuantity != null) ? item.RetItemQuantity : 0;
                                    var DvItemQuantity = (item.DvItemQuantity != null) ? item.DvItemQuantity : 0;
                                    var itmbal = (parseFloat(DvItemQuantity).toFixed(2) - parseFloat(RetItemQuantity)).toFixed(2);
                                    qtySum += parseFloat(itmbal);
                                });

                                if (qtySum > 0) {
                                    $.each(data, function (i, item) {
                                        var chckv = 0;
                                        var RetItemQuantity = (item.RetItemQuantity != null) ? item.RetItemQuantity : 0;
                                        var DvItemQuantity = (item.DvItemQuantity != null) ? item.DvItemQuantity : 0;
                                        var itmbalance = (parseFloat(DvItemQuantity) - parseFloat(RetItemQuantity)).toFixed(2);
                                        
                                        $("#addinvoiceItem tr.item_" + item.Item).each(function () {
                                            var itunit = $(this).find('.units').val();
                                            var subitem = $(this).find(".itemdisc").val();
                                            if (itunit == item.ItemUnit && subitem == 0) {
                                                chckv = 1;
                                            }
                                        });
                                        
                                        if (chckv == 0 && itmbalance > 0) {
                                            //alert(chckv + 'f' + itmbalance)
                                            var tbodypurchase = $("#normalinvoice tbody");
                                            tbodypurchase.children("tr").each(function () {
                                                var rowid = $(this).attr("id");
                                                var classname = $(this).closest('tr').attr('class');
                                                if (classname == 'item_') {
                                                    $(this).closest("tr").remove();
                                                }
                                                
                                            });
                                            addrow('addinvoiceItem', '', item.ItemUnit, itmbalance, RetItemQuantity, DvItemQuantity, itmbalance, item.Item, item.ItemCode, item.ItemName, item.ItemWithCode, item.ItemNote, item, item.ItemDiscount, itmbalance, 0, 0, item.BaseQty, invoice);

                                            var qtySumBundle = 0;
                                            $.each(item.bundle, function (i, item) {
                                                var RetItemQuantitys = (item.RetItemQuantity != null) ? item.RetItemQuantity : 0;
                                                var DvItemQuantitys = (item.DvItemQuantity != null) ? item.DvItemQuantity : 0;
                                                var itmbals = (parseFloat(DvItemQuantity).toFixed(2) - parseFloat(RetItemQuantity)).toFixed(2);
                                                qtySumBundle += parseFloat(itmbals);
                                            });

                                            if (qtySumBundle > 0) {
                                                $.each(item.bundle, function (i, item) {
                                                    var chckvs = 0;
                                                    var RetItemQuantitys = (item.RetItemQuantity != null) ? item.RetItemQuantity : 0;
                                                    var DvItemQuantitys = (item.DvItemQuantity != null) ? item.DvItemQuantity : 0;
                                                    var itmbalances = (parseFloat(DvItemQuantity * item.BaseQty) - parseFloat(RetItemQuantitys)).toFixed(2);
                                                    var itQty = 
                                                    $("#addinvoiceItem tr.item_" + item.Item).each(function () {
                                                        var itunit = $(this).find('.units').val();
                                                        if (itunit == item.ItemUnit) {
                                                            chckvs = 1;
                                                        }
                                                    });

                                                    if (chckvs == 0 && itmbalances > 0) {
                                                        addrow('addinvoiceItem', '', item.ItemUnit, itmbalances, RetItemQuantitys, DvItemQuantity * item.BaseQty, itmbalances, item.Item, item.ItemCode, item.ItemName, item.ItemWithCode, item.ItemNote, item, item.ItemDiscount, itmbalances, 0, 0, item.BaseQty, invoice);
                                                    }
                                                });
                                            }
                                        }

                                        rowSubTotal(item.Item);
                                        CalculatetblItemListSum();
                                        HireItemTotal();
                                    });
                                } else {
                                    $('#ReturnTable').hide();
                                    alert("No Items Pending !!");
                                }

                                addrow('addinvoiceItem', '', '', '0.00', '0.00', '0');
                            } else {
                                $('#addinvoiceItem').html('');
                                addrow('addinvoiceItem', '', '', '0.00', '0.00', '0');
                            }
                        }
                    });


                }
            }
        });
    }
}

function BindDataInvoiceCross(invoice) {
    //alert(invoice+' fgdfsdf')
    if (invoice != null) {
        $.ajax({
            url: '/CrossHireReturn/GetCrossHireType',
            type: "POST",
            dataType: "JSON",
            data: { Invoice: invoice },
            success: function (result) {
                $("#ddlHType").val(result.Hty);
                if (result.Hty != null) {
                    $('#ReturnTable').show();
                    $.ajax({
                        url: '/PurchaseEntry/GetPEItemsHire',
                        dataType: 'json',
                        data: { PurchaseEntryID: invoice },
                        cache: true,
                        success: function (data) {
                            if (data != "") {
                                var qtySum = 0;
                                //$('#addinvoiceItem').html('');
                                $.each(data, function (i, item) {
                                    var RetItemQuantity = (item.RetItemQuantity != null) ? item.RetItemQuantity : 0;
                                    var DvItemQuantity = (item.DvItemQuantity != null) ? item.DvItemQuantity : 0;
                                    var itmbal = (parseFloat(DvItemQuantity).toFixed(2) - parseFloat(RetItemQuantity)).toFixed(2);
                                    qtySum += parseFloat(itmbal);
                                });

                                if (qtySum > 0) {
                                    $.each(data, function (i, item) {
                                        var chckv = 0;
                                        var RetItemQuantity = (item.RetItemQuantity != null) ? item.RetItemQuantity : 0;
                                        var DvItemQuantity = (item.DvItemQuantity != null) ? item.DvItemQuantity : 0;
                                        var itmbalance = (parseFloat(DvItemQuantity) - parseFloat(RetItemQuantity)).toFixed(2);

                                        $("#addinvoiceItem tr.item_" + item.Item).each(function () {
                                            var itunit = $(this).find('.units').val();
                                            var subitem = $(this).find(".itemdisc").val();
                                            if (itunit == item.ItemUnit && subitem == 0) {
                                                chckv = 1;
                                            }
                                        });

                                        if (chckv == 0 && itmbalance > 0) {
                                            //alert(chckv + 'f' + itmbalance)
                                            var tbodypurchase = $("#normalinvoice tbody");
                                            tbodypurchase.children("tr").each(function () {
                                                var rowid = $(this).attr("id");
                                                var classname = $(this).closest('tr').attr('class');
                                                if (classname == 'item_') {
                                                    $(this).closest("tr").remove();
                                                }

                                            });
                                            addrow('addinvoiceItem', '', item.ItemUnit, itmbalance, RetItemQuantity, DvItemQuantity, itmbalance, item.Item, item.ItemCode, item.ItemName, item.ItemWithCode, item.ItemNote, item, item.ItemDiscount, itmbalance, 0, 0, item.BaseQty, invoice);

                                            var qtySumBundle = 0;
                                            $.each(item.bundle, function (i, item) {
                                                var RetItemQuantitys = (item.RetItemQuantity != null) ? item.RetItemQuantity : 0;
                                                var DvItemQuantitys = (item.DvItemQuantity != null) ? item.DvItemQuantity : 0;
                                                var itmbals = (parseFloat(DvItemQuantity).toFixed(2) - parseFloat(RetItemQuantity)).toFixed(2);
                                                qtySumBundle += parseFloat(itmbals);
                                            });

                                            if (qtySumBundle > 0) {
                                                $.each(item.bundle, function (i, item) {
                                                    var chckvs = 0;
                                                    var RetItemQuantitys = (item.RetItemQuantity != null) ? item.RetItemQuantity : 0;
                                                    var DvItemQuantitys = (item.DvItemQuantity != null) ? item.DvItemQuantity : 0;
                                                    var itmbalances = (parseFloat(DvItemQuantity * item.BaseQty) - parseFloat(RetItemQuantitys)).toFixed(2);
                                                    var itQty =
                                                    $("#addinvoiceItem tr.item_" + item.Item).each(function () {
                                                        var itunit = $(this).find('.units').val();
                                                        if (itunit == item.ItemUnit) {
                                                            chckvs = 1;
                                                        }
                                                    });

                                                    if (chckvs == 0 && itmbalances > 0) {
                                                        addrow('addinvoiceItem', '', item.ItemUnit, itmbalances, RetItemQuantitys, DvItemQuantity * item.BaseQty, itmbalances, item.Item, item.ItemCode, item.ItemName, item.ItemWithCode, item.ItemNote, item, item.ItemDiscount, itmbalances, 0, 0, item.BaseQty, invoice);
                                                    }
                                                });
                                            }
                                        }

                                        rowSubTotal(item.Item);
                                        CalculatetblItemListSum();
                                        HireItemTotal();
                                    });
                                } else {
                                    $('#ReturnTable').hide();
                                    alert("No Items Pending !!");
                                }

                                addrow('addinvoiceItem', '', '', '0.00', '0.00', '0');
                            } else {
                                $('#addinvoiceItem').html('');
                                addrow('addinvoiceItem', '', '', '0.00', '0.00', '0');
                            }
                        }
                    });


                }
            }
        });
    }
}

function BindInvoicesCrossHire(cust) {
    $('#ddlInvoice').val(null).trigger('change');
    var project = $('#ddlProject').val();
    //bind to salesentry
    $("#ddlInvoice").select2({
        placeholder: 'Search Hire Invoices ',
        minimumInputLength: 0,
        ajax: {
            url: "/PurchaseEntry/SearchCrossHireEntry",
            dataType: 'json',
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    cust: cust,
                    project: project
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