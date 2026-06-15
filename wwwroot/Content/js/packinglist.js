var count = 1, type = '';
limits = 500;
//Add Row
function addrow(t, action, ItemUnit, ItemQuantity, Item, ItemCode, ItemName, ItemWithCode, ItemNote, itemdata, type, BaseQty, ItemDiscount, pkt,MinQty) {
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
        var itselect = "";

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
            itselect = "<select class='form-control' " + required + " data-id='" + count + "' id='item_name_" + count + "'>" + Option + "</select>"
        } else {
            itselect = "<select class='form-control item_name' " + required + " data-id='" + count + "' placeholder='Item Name' id='item_name_" + count + "'  data-msg-required='The Item Name is required' onchange='GetItemdetails(this," + count + ",\"" + type + "\")'>" + Option + "</select>"
        }
        if (count == 1) {
            required = 'required="required"';
        }
        var inote = "";
        var readonly = "";
        var readonlyqty = "";
        var deletebtn = "<button tabindex='" + tab7 + "' style='text-align: right;' class='btn btn-danger' type='button' value='Delete' onclick='deleteRow(this," + Item + ")'><i class='fa fa-trash fa-1x'></i></button> ";
        var pkts = pkt != null ? pkt : (type != null) ? 1 : 0;
        var minqty = MinQty != null ? MinQty : (type != null) ? 1 : 0;
        notbtn = "<button type='button' class='itnote btn btn-default btn-flat' data-toggle='modal' data-target='#modal-item-" + count + "'><i class='fa fa-1x fa-file-text-o'></i></button>";

        if (type == "bundle") {
            readonlyqty = "readonly='readonly'";
            deletebtn = "";
            notbtn ="";
        }
        else if (type == "Main Item") {
            pkts = "";
            minqty = "";
            readonly = "readonly='readonly'";
        } 

        itemnote = '<div id="modal-item-' + count + '" class="modal fade" role="dialog" aria-hidden="true"><div class="modal-dialog"><div class="modal-content">' +
            '<div class="form-group"><textarea name="itemnote" cols="40" rows="10" class="form-control itemnote" id="itemnote-' + count + '">' + inote + '</textarea></div>' +
            '<div class="form-group"><button class="btn btn-info" type="button" data-dismiss="modal">Save</button></div>' +
            '</div></div></div>';
        var itemaddbtn = "<span class='input-group-btn'>" + notbtn + "</span>";


        Item = (typeof Item == 'undefined') ? 0 : Item;

        data = "<td class='text-center' id=" + divid + "> " + slno + " </td>" +
                "<td class='input-group input-group-sm' style='max-width:400px;'> " + itselect + itemaddbtn + "</td>" +
                "<td style='width:100px;'><select class='form-control units unit_name_" + count + "' id='unit_name_" + count + "' " + required + " data-id='" + count + "'></select></td>" +
                "<td> <input type='number' name='product_quantity[]' onchange='quantity_change(" + count + ");' id='product_quantity_" + count + "' value='" + parseFloat(ItemQuantity).toFixed(2) + "'  class='total_qntt_" + count + " form-control text-right quty' placeholder='0' min='0' tabindex='" + tab2 + "' " + readonlyqty + " /></td>" +
                "<td><input type='number' name='product_packet[]' onchange='pkqty_change(" + count + ");' id='product_packet_" + count + "' class='product_packet_" + count + " form-control text-right pktqty' value='" + pkts + "' min='0' tabindex='" + tab2 + "'" + readonly + "  /></td>" +
                "<td><input type='number' name='product_minqty[]' onchange='minqty_change(" + count + ");' id='product_minqty_" + count + "' class='product_minqty_" + count + " form-control text-right minqty' value='" + minqty + "' min='0' tabindex='" + tab2 + "'" + readonly + "  /></td>" +

                "<td class='text-center'>" + deletebtn + itemnote + htdata +
                "<input type='hidden' class='item_disc_" + count + " itemdisc' id='item_disc_" + count + "' value='" + ItemDiscount + "'/>" +
                "<input type='hidden' class='item_id_" + count + " itemid' id='item_id_" + count + "' value='" + Item + "'/>" +
                "<input type='hidden' class='baseqty_" + count + " baseqty' id='baseqty_" + count + "' value='" + BaseQty + "'/>"+
                "<input type='hidden' class='itemtype_" + count + " itemtype' id='itemtype_" + count + "' value='" + type + "'/></td>";
        row += data + "</tr>";
        $('#' + t).append(row);
        searchItem();
        
        if (itemdata && type != "bundle") {
            createUnitList(itemdata, count);
        }
        else if (type == "bundle") {
            createBundleUnitList(itemdata, count);
        }
        if (action == "edit") {
            $("#actionvalue").val(action);
        } else {
            $("#actionvalue").val("");
        }

        $("#item_name_"+ count).css("width", "400px");

        count++;
        setTabIndex();
    }
}

//item details
function GetItemdetails(selectObject, dataid, action) {
    if (selectObject.value) {
        var ItemId = selectObject.value;
        if (ItemId != null) {
            if ($(".item_" + ItemId).length > 0) {
                alert("Sorry You Cant Add An Item More Than One Time");
                $(selectObject).val(null).trigger('change');
            }
            else {
                itemUpdate(selectObject, dataid, action);
            }
        }
    }

}
// update item details
function itemUpdate(selectObject, dataid, action) {
    var sentry = "";
    var itemid = "";
    var url = "";
    sentry = getQueryString('');
    if (action == "edit") {
        url = '/PackingList/SearchPackItemById';
        itemid = selectObject !=null ? selectObject.value : null;
    } else {
        url = '/PackingList/SearchItemById';
        itemid = selectObject.value;
    }
    $.ajax({
        url: url,
        type: "POST",
        dataType: 'json',
        data: { ItemId: itemid, entryId: sentry },
        cache: true,
        success: function (data) {
            if (data.length>0) {
                var qtySum = 0;
                $(".item_").remove();
                $.each(data, function (i, item) {
                    var chckv = 0;
                    var type = item.bundle != null ? (item.bundle.length > 0 ? "Main Item" : "Item") : "Item";
                    if (action == "edit") {
                        var tbody = $("#normalinvoice tbody");
                        if (tbody.children().length > 0) {
                            tbody.children("tr").each(function () {
                                var rowid = $(this).attr("id");
                                var itemid = $("#" + rowid + " .itemid").val();
                                if (itemid == "0.00") {
                                    $(this).closest("tr").remove();
                                }
                            });
                        }
                        addrow('addinvoiceItem', '', item.ItemUnit, item.ItemQuantity, item.Item, item.ItemCode, item.ItemName, item.ItemWithCode, item.ItemNote, item, type, item.BaseQty, item.ItemDiscount, item.Packet, item.MinQty);
                    } else {
                        addrow('addinvoiceItem', '', item.ItemUnit, 1, item.Item, item.ItemCode, item.ItemName, item.ItemWithCode, item.ItemNote, item, type, item.BaseQty, item.ItemDiscount, item.Packet, item.MinQty);
                    }
                    $.each(item.bundle, function (i, item1) {
                        if (action == "edit") {
                            addrow('addinvoiceItem', '', item1.ItemUnit, item1.BaseQty, item1.Item, item1.ItemCode, item1.ItemName, item1.ItemWithCode, item1.ItemNote, item1, "bundle", item1.BaseQty, item1.ItemDiscount, item1.Packet, item1.MinQty);
                            
                        } else {
                            addrow('addinvoiceItem', '', item1.ItemUnit, item1.BaseQty, item1.Item, item1.ItemCode, item1.ItemName, item1.ItemWithCode, item1.ItemNote, item1, "bundle", item1.BaseQty, item1.ItemDiscount, item1.Packet, item1.MinQty);
                        }
                    });
                    CalculatetblItemListSum();
                });
                addrow('addinvoiceItem', 'pkt', "", "0.00");
            } else {
                addrow('addinvoiceItem', 'pkt', "", "0.00");
            }
        }
    });
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

function createBundleUnitList(result, dataid) {
    // clear previous content
    $('#unit_name_' + dataid).empty();
    if (result.ItemUnit != null) {
        var newOption = $('<option></option>');
        newOption.val(result.ItemUnit).html(result.ItemUnitName);           
        $('#unit_name_' + dataid).append(newOption);            
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
function repoFormatResult(repo) {
    var bg = "";
    var markup = '<div class="se-row' + bg + '">' +
             '<h4>' + repo.text + '</h4>';
    if (repo.PartNumber != "" && repo.PartNumber != null) {
        markup += '<div class="se-sec">Part No : ' + repo.PartNumber + '</div>,';
    }
    if (repo.price)
        markup += '<div class="se-sec">Price:' + parseFloat(repo.price).toFixed(2) + '</div>';
    if (repo.cost)
        markup += '<div class="se-sec">Cost:' + parseFloat(repo.cost).toFixed(2) + '</div>';
    markup += '</div>';
    var retn = $(markup);
    return retn;
}
function repoFormatSelection(repo) {
    return repo.text;
}
function quantity_change(arg) {
    var qty = $(".total_qntt_" + arg).val() || 0;
    var itemid = $(".item_id_" + arg).val();
    var tbody = $("#normalinvoice tbody");
    if (tbody.children().length > 0) {
        tbody.children("tr").each(function () {
            var rowid = $(this).attr("id");
            var subitem = $("#" + rowid + " .itemdisc").val();
            var baseqty = $("#" + rowid + " .baseqty").val();

            var totalqty = qty * baseqty;

            if (subitem == itemid) {
                $("#" + rowid + " .quty").val(totalqty.toFixed(2));
            }
        });
    }
    CalculatetblItemListSum();
}
function pkqty_change(arg) {
    var pkt = $(".product_packet_" + arg).val() || 0;
    var qty = $(".total_qntt_" + arg).val() || 0;
    var minqty = qty / pkt;

    if (minqty % 1 != 0) {
        $(".product_minqty_" + arg).val(Math.trunc(minqty) + 1);
    } else
    {
        $(".product_minqty_" + arg).val(minqty);
    }
    CalculatetblItemListSum();
}
function minqty_change(arg) {
    CalculatetblItemListSum();
}

function CalculatetblItemListSum() {
    var qty = $(".quty").val();
    var tbody = $("#normalinvoice tbody");
    if (tbody.children().length > 0) {
        var gtQty = 0;
        var pkQty = 0;
        var minQty = 0;
        $(".quty").each(function () {
            var subQty = this.value;
            subQty = subQty || 0;
            gtQty = parseFloat(gtQty) + parseFloat(subQty);
        });

        $(".pktqty").each(function () {
            var subQty = this.value;
            subQty = subQty || 0;
            pkQty = parseFloat(pkQty) + parseFloat(subQty);
        });

        $(".minqty").each(function () {
            var subQty = this.value;
            minQty = minQty || 0;
            minQty = parseFloat(minQty) + parseFloat(subQty);
        });
        
        $("[id$=ItemCount]").val(tbody.children().length);
        $("[id$=ItemQty]").text((gtQty).toFixed(2));
        $("[id$=PktQty]").text((pkQty).toFixed(2));
        $("[id$=MinQty]").text((minQty).toFixed(2));
    }
}
//Delete a row of table
function deleteRow(t, item) {
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
            //Row += dvField1;
            //Row += dvField2;

            Row += '<td><b>' + ritem.Packet.toFixed(2) + '</b></td>';
            //Row += '<td><b>' + ritem.MinQty.toFixed(2) + '</b></td>';
        }
        Row += '</tr>';
        return Row;
    }
    $.each(e.item, function (i, item) {
        qty += item.ItemQuantity;

        //var subtot = parseFloat(item.ItemTotalAmount.toFixed(2));
        //total += subtot;

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
       // str += "<tr class='border-top'><td colspan='3' class='text-right'><b>TOTAL</b></td><td class='text-center'><b>" + weihtv + "</b></td><td class='text-center'><b>" + cbmv + "</b></td><td></td><td></td><td></td><td colspan='2'></td><td></td></tr>";
       // str += "<tr class='border-top'><td colspan='5' class='text-right'><b>TOTAL</b></td><td class='text-right'>" + parseFloat(QtyTot).toFixed(2) + "</td><td></td><td class='text-right'>" + parseFloat(TotTaxableAmount).toFixed(2) + "</td><td colspan='2' class='text-right'>" + parseFloat(TotTaxAmount).toFixed(2) + "</td><td class='text-right'>" + parseFloat(GrandTot).toFixed(2) + "</td></tr>";
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
    //if (e.summary.ProCheck == 0 && (e.summary.PrjNameCode != null && e.summary.PrjNameCode != "")) {
    //    $("#lblProject").text(e.summary.PrjNameCode);
    //}
    //else {
    //    $("#divproject").hide();
    //}

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

    if (e.fmapp!=null) {
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

    var tc = (e.summary.TermsCondition != null) ? e.summary.TermsCondition.replace(/\n/g, "<br/>") : "";
    var remark =  (e.summary.Remarks!= null)?e.summary.Remarks.replace(/\n/g, "<br/>"):"";
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
                        "<tr><th style='width:28% !important;'>MOBILE NO</th><td>:</td><td>" + CMobile + "</td></tr>" +
                        "<tr><th style='width:28% !important;'>EMAIL</th><td>:</td><td>" + CEmail + "</td></tr>" +
                        "<tr><th style='width:28% !important;'>TRN</th><td>:</td><td>" + CTRN + "</td></tr>";

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
        case 'PackingList':
            var titname = "Packing List - " + e.summary.PartyName + " - " + e.summary.BillNo;
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

    setTimeout(function () { window.print(); }, e.summary.TimeOut);
}


function fieldReset() {
    $("#ItemQty").text(0.00);
    $("#PktQty").text(0.00);
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

