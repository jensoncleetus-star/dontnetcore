var count = 1, type = '';
limits = 500;
function addrow(t, action, ItemUnit, ItemTax, ItemTotalAmount, ItemQuantity, Item, ItemCode, ItemName, ItemUnitPrice, ItemSubTotal, ItemWithCode, CsStock, ItemUnitId, items) {
    if (count == limits) alert("You have reached the limit of adding " + count + " inputs");
    else {
        var data = "";
        var Type = "";
        var Option = "";
        var readonly = "";
        var row = "<tr class='item_' id='item_" + count + "'>";
        var slno = $('#addinvoiceItem tr').length + 1;
        var a = "item_name" + count,
        tabindex = count * 4;
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
        if (count == 1) {
            required = 'required="required"';
        }
        if (action != '') {
            type = action;
        }

        ItemUnit = ItemUnit != null ? ItemUnit : "";
        var CSPics = "";
        var CSQty = "";
        var PSPics = "";
        var PSQty = "";
        var SDPics = "";
        var SDQty = "";
        if (action == "") {
            CSPics = CsStock != null ? CsStock : 0;
            CSQty = CsStock != null ? CsStock : 0;
            //PSPics = ItemQuantity != null ? ItemQuantity : 0;
            //PSQty = ItemQuantity != null ? ItemQuantity : 0;

            PSPics = 1;// CsStock != null ? CsStock : 0;
            PSQty = 1;// CsStock != null ? CsStock : 0;
            SDPics = CsStock - ItemQuantity;
            SDQty = CsStock - ItemQuantity;
        } else {
            CSPics = items.CSPcs;
            CSQty = items.CSqty;
            PSPics = items.PSPcs;
            PSQty = items.PSqty;
            SDPics = items.SDPcs;
            SDQty = items.SDqty;
        }


        data = "<td class='text-center'> " + slno + " </td>" +
                "<td><input type='text' data-name='ItemCode' name='' id='ItemCode_" + count + "' value='" + ItemCode + "'  class='ItemCode_" + count + " form-control itemcode' tabindex='" + tab1 + "' readonly/></td>" +
                "<td><input type='text' data-name='ItemName' name='' id='ItemName_" + count + "' value='" + ItemName + "'  class='ItemName_" + count + " form-control itemname' tabindex='" + tab2 + "' readonly/></td>" +
                "<td><input type='text' data-name='ItemUnit' id='ItemUnit_" + count + "' value='" + ItemUnit + "'  class='ItemUnit_" + count + " form-control itemunit' tabindex='" + tab3 + "' readonly/></td>" +

                "<td style='display:none'> <input type='number' data-name='CSPcs' id='CSPics_" + count + "' value='" + CSPics + "'  class='CSPics_" + count + " form-control text-right cspics' placeholder='0' value='0' min='.01' tabindex='" + tab4 + "' readonly/></td>" +
                "<td> <input type='number' data-name='CSqty'  id='CSQty_" + count + "' value='" + CSQty + "'  class='CSQty_" + count + " form-control text-right csqty' placeholder='0' value='0' min='.01' tabindex='" + tab5 + "' readonly/></td>" +

                "<td style='display:none'> <input type='number' data-name='PSPcs' id='PSPics_" + count + "' onchange='pspics_change(" + count + ");' value='" + PSPics + "'  class='PSPics_" + count + " form-control text-right pspics' placeholder='0' value='0' min='.01' tabindex='" + tab6 + "'/></td>" +
                "<td> <input type='number' data-name='PSqty'  id='PSQty_" + count + "' onchange='psqty_change(" + count + ");' value='" + PSQty + "'  class='PSQty_" + count + " form-control text-right psqty' placeholder='0' value='0' min='.01' tabindex='" + tab7 + "'/></td>" +

                "<td style='display:none'> <input type='number' data-name='SDPcs' id='SDPics_" + count + "' value='" + SDPics + "'  class='CSPics_" + count + " form-control text-right sdpics' placeholder='0' value='0' min='.01' tabindex='" + tab8 + "' readonly/></td>" +
                "<td> <input type='number' data-name='SDqty'  id='SDQty_" + count + "' value='" + SDQty + "'  class='SDQty_" + count + " form-control text-right sdqty' placeholder='0' value='0' min='.01' tabindex='" + tab9 + "' readonly/>" +
                "<input type='hidden' class='ItemUnitId' data-name='ItemUnitId' id='ItemUnitId_" + count + "' value='" + ItemUnitId + "'/> " +
            "<input type='hidden' class='itemid' data-name='Item' id='itemid_" + count + "' value='" + Item + "'/></td>" +
            "<input type='hidden' class='Barcode' data-name='Barcode' id='Barcode_" + count + "' value='" + ItemTotalAmount + "'/></td>" +
                "<td class='text-center'><button tabindex='" + tab4 + "' style='text-align: right;' class='btn btn-danger' type='button' value='Delete' onclick='deleteRow(this)'><i class='fa fa-trash fa-1x'></i></button></td>";
        row += data + "</tr>";

        pspics_change(count);
        psqty_change(count);

        $('#' + t).append(row);
        //if (itemdata) {
        //    createUnitList(itemdata, count);
        //}
        ColumnTotal();
        count++;
        setTabIndex();

    }
}
function pspics_change(arg) {

    var cspics = $("#CSPics_" + arg).val();
    var pspics = $("#PSPics_" + arg).val();
    $("#PSPics_" + arg).val(pspics);
    $("#PSQty_" + arg).val(pspics);

    var newpics = parseFloat(cspics) - parseFloat(pspics);
    $("#SDPics_" + arg).val(newpics.toFixed(2));
    $("#SDQty_" + arg).val(newpics.toFixed(2));
    ColumnTotal();
}
function psqty_change(arg) {
    var csqty = $("#CSQty_" + arg).val();
    var psqty = $("#PSQty_" + arg).val();
    $("#PSPics_" + arg).val(psqty);
    $("#PSQty_" + arg).val(psqty);

    var newqty = parseFloat(csqty) - parseFloat(psqty);
    $("#SDQty_" + arg).val(newqty.toFixed(2));
    $("#SDPics_" + arg).val(newqty.toFixed(2));
    ColumnTotal();
}
function ColumnTotal() {
    var tbody = $("#normalinvoice tbody");
    if (tbody.children().length > 0) {
        var cspics = 0;
        var csqty = 0;
        var pspics = 0;
        var psqty = 0;
        var sdpics = 0;
        var sdqty = 0;
        $(".cspics").each(function () {
            var subQty1 = this.value;
            cspics = parseFloat(cspics) + parseFloat(subQty1);
        });

        $(".csqty").each(function () {
            var subQty2 = this.value;
            csqty = parseFloat(csqty) + parseFloat(subQty2);
        });

        $(".pspics").each(function () {
            var subQty3 = this.value;
            pspics = parseFloat(pspics) + parseFloat(subQty3);
        });

        $(".psqty").each(function () {
            var subQty4 = this.value;
            psqty = parseFloat(psqty) + parseFloat(subQty4);
        });

        $(".sdpics").each(function () {
            var subQty5 = this.value;
            subQty5 = subQty5 || 0;
            sdpics = parseFloat(sdpics) + parseFloat(subQty5);
        });

        $(".sdqty").each(function () {
            var subQty6 = this.value;
            subQty6 = subQty6 || 0;
            sdqty = parseFloat(sdqty) + parseFloat(subQty6);
        });

        $("#CSPcsT").text((cspics).toFixed(2));
        $("#CSQtyT").text((csqty).toFixed(2));
        $("#PSPcsT").text((pspics).toFixed(2));
        $("#PSQtyT").text((psqty).toFixed(2));
        $("#SDPcsT").text((sdpics).toFixed(2));
        $("#SDQtyT").text((sdqty).toFixed(2));

        var totpcs = $("#totPCS").text();
        var remqty = parseFloat(totpcs) - parseFloat(psqty);
        $("#scnPCS").text((psqty).toFixed(2));
        $("#remPCS").text((remqty).toFixed(2));

    }
}


// update item details
function itemUpdate(itemId,mcs) {
    $.ajax({
        url: '/Item/GetItemMC',
        dataType: 'json',
        data: { itemID: itemId,mc:mcs },
        cache: true,
        success: function (item) {
            $("#ddlItem").select2("val", "");
            var chk = false;
            $(".item_").remove();
            var tbody = $("#normalinvoice tbody");
            if (tbody.children().length > 0) {
                tbody.children("tr").each(function () {
                    var rowid = $(this).attr("id");
                    var item = $("#" + rowid + " .itemid").val();
                    var qty = $("#" + rowid + " .psqty").val();
                    if (item != null) {
                        if (item == itemId) {
                            chk = true;
                            qty++;
                            $("#" + rowid + " .pspics").val(qty);
                            $("#" + rowid + " .psqty").val(qty);

                            var cspics = $("#" + rowid + " .cspics").val();
                            var csqty = $("#" + rowid + " .csqty").val();

                            var newpics = cspics - qty;
                            var newqty = csqty - qty;

                            $("#" + rowid + " .sdpics").val(newpics);
                            $("#" + rowid + " .sdqty").val(newqty);
                        }
                    }
                });
            }
            if (chk == false) {
                addrow('addinvoiceItem', '', item.PriUnit, item.Tax, '', 1, item.ItemID, item.ItemCode, item.ItemName, item.SellingPrice, item.SellingPrice, item.ItemWithCode, item.total, item.ItemUnitID);
            }
            addrow('addinvoiceItem', '', "", "0.00", "0.00", "0", '', '', '');
            $(".barcode").val("");
            $(".barcode").focus();
        }
    });
}





function itemUpdatebarcode(itemId, mcs) {
    $.ajax({
        url: '/Item/GetItemMCbar',
        dataType: 'json',
        data: { itemID: itemId, mc: mcs },
        cache: true,
        success: function (item) {
            $("#ddlItem").select2("val", "");
            var chk = false;
            $(".item_").remove();
            var tbody = $("#normalinvoice tbody");
            if (tbody.children().length > 0) {
                tbody.children("tr").each(function () {
                    var rowid = $(this).attr("id");
                    var item = $("#" + rowid + " .Barcode").val();
                    var qty = $("#" + rowid + " .psqty").val();
                    if (item != null) {
                        if (item == itemId) {
                            chk = true;
                            qty++;
                            $("#" + rowid + " .pspics").val(qty);
                            $("#" + rowid + " .psqty").val(qty);

                            var cspics = $("#" + rowid + " .cspics").val();
                            var csqty = $("#" + rowid + " .csqty").val();

                            var newpics = cspics - qty;
                            var newqty = csqty - qty;

                            $("#" + rowid + " .sdpics").val(newpics);
                            $("#" + rowid + " .sdqty").val(newqty);
                        }
                    }
                });
            }
            if (chk == false) {
                addrow('addinvoiceItem', '', item.PriUnit, item.Tax, item.Barcode, 1, item.ItemID, item.ItemCode, item.ItemName, item.SellingPrice, item.SellingPrice, item.ItemWithCode, item.total, item.ItemUnitID);
            }
            addrow('addinvoiceItem', '', "", "0.00", "0.00", "0", '', '', '');
            $(".barcode").val("");
            $(".barcode").focus();
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
    $('#addbomitem tr').each(function () {
        $(this).find('td:first').text(i);
        i++;
    });
    ColumnTotal();
}

function StkSubmit(fnval) {
    var HTMLtbl = {
        getData: function (table) {
            var data = [];
            table.find('tr').not(':first').not('.chkhead').not('.item_').each(function (rowIndex, r) {
                var cols = {};
                $(this).find('input,textarea,select').each(function (colIndex, c) {
                    itid = $(this).attr('data-name');
                    itval = ($(this).val() != "") ? $(this).val() : $(this).text();
                    cols[itid] = itval;
                });
                data.push(cols);
            });
            return data;
        }
    }

    var data = HTMLtbl.getData($('#normalinvoice'));
    var parameters = {};
    parameters.id = $('#StockVerificationId').val();

    parameters.Voucher = $('#Voucher').val();
    parameters.Date = $('#Date').val();

    parameters.Remarks = $('#Remarks').val();
    parameters.Note = $('#Note').val();

    parameters.action = fnval;
    parameters.svItemzz = data;

    var url = "";
    if (fnval == "save") {
        url = "/StockVerification/Create";
    }
    if (fnval == "update") {
        url = "/StockVerification/Edit";
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
            if (e.status == true) {
                $('.ajax_response', res_success).text(e.message);
                $('.AlertDiv').prepend(res_success);
                if (fnval != null) {
                    if (fnval == "print") {
                        printInvoice(e);
                    }
                    window.location.href = '/StockVerification/Index';
                } else {
                    location.reload();
                }
                $("button").prop('disabled', false); // enable button
            } else {
                $('.ajax_response', res_danger).text(e.message);
                $('.AlertDiv').prepend(res_danger);
                $("button").prop('disabled', false); // enable button
            }
        }
    });
}
function printInvoice(e) {
    $("#lblBillNo").text(e.summary.Voucher);
    $("#lblDate").text(convertToDate(e.summary.Date));

    var itemsData = bindItem(e);
    $('#itemtable tbody').html("");
    $('#itemtable').append(itemsData);

    var originalpage = document.body.innerHTML;
    var printContent = $('#printit').html();
    $('body').html(printContent);
    window.print();
}
function bindItem(e) {
    var str = "";
    var count1 = 1;
    var count2 = 1;
    $.each(e.item, function (i, item) {
        //var itemnote = "";
        //if (item.Note != "") {
        //    itemnote = "<br /><small>" + item.Note + "</small>";
        //}
        var unit = (item.ItemUnit != null) ? item.ItemUnit : "";
        str += '<tr>';
        str += '<td>' + count1 + '</td>';
        str += '<td>' + item.ItemCode + '</td>';
        str += '<td>' + item.ItemName + '</td>';
        str += '<td>' + unit + '</td>';
        str += '<td>' + item.CSPcs.toFixed(2) + '</td>';
        str += '<td>' + item.CSQty.toFixed(2) + '</td>';
        str += '<td>' + item.PSPcs.toFixed(2) + '</td>';
        str += '<td>' + item.PSQty.toFixed(2) + '</td>';
        str += '<td>' + item.SDPcs.toFixed(2) + '</td>';
        str += '<td>' + item.SDQty.toFixed(2) + '</td>';
        str += '</tr>';
        count1++;
    });
    if (e.pstock != null) {
        str += '<tr><td colspan="10" class="text-center"><b>Pending Stock</b><td></tr>';
        $.each(e.pstock, function (i, item) {
            //var itemnote = "";
            //if (item.Note != "") {
            //    itemnote = "<br /><small>" + item.Note + "</small>";
            //}
            var unit = (item.ItemUnit != null) ? item.ItemUnit : "";
            str += '<tr>';
            str += '<td>' + count2 + '</td>';
            str += '<td>' + item.ItemCode + '</td>';
            str += '<td>' + item.ItemName + '</td>';
            str += '<td>' + unit + '</td>';
            str += '<td>' + item.RemainQty.toFixed(2) + '</td>';
            str += '<td>' + item.RemainQty.toFixed(2) + '</td>';
            str += '<td>0.00</td>';
            str += '<td>0.00</td>';
            str += '<td>' + item.RemainQty.toFixed(2) + '</td>';
            str += '<td>' + item.RemainQty.toFixed(2) + '</td>';
            str += '</tr>';
            count2++;
        });
    }
    return str;
}
function fillAmounts() {
    $.ajax({
        url: '/Item/GetTotalStock',
        dataType: 'json',
        cache: true,
        success: function (item) {
           // $("#totPCS").text(item.toFixed(2));
            var scnpc = $("#scnPCS").text();
            if (scnpc != 0) {
                var rempc = parseFloat(item) - parseFloat(scnpc);
               // $("#remPCS").text(rempc.toFixed(2));
            } else {
                $("#scnPCS").text("0.00");
             //   $("#remPCS").text("0.00");
            }

        }
    });

    
}