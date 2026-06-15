var count = 1, type = '';
var bcount = 1;
var generatedcount = 1;
limits = 500;
//production invoice
//item
function bomitem(t, action, Quantity, Unit, Item, ItemWithCode, itemdata, Bomid) {
    if (count == limits) alert("You have reached the limit of adding " + count + " inputs");
    else {
        var data = "";
        var Type = "";
        var Option = "";
        var readonly = "";
        var required = "";
        var row = "<tr class='item_' id='item_" + count + "'>";
        var slno = $('#addbomitem tr').length + 1;
        var a = "item_name" + count,
        tabindex = count * 4;
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
        if (action != '') {
            type = action;
        }
        var Price = 0;
        var Amount = 0;
        var htdata = "";
        if (itemdata) {
            if (Unit == itemdata.ItemUnitID) {
                Price = (itemdata.price);
            }
            if (Unit == itemdata.SubUnitId) {
                Price = ((itemdata.price) / itemdata.ConFactor);
            }
            
            Amount = (Quantity * Price);

            //minimum stock check
            htdata = "<div class='mstock minstock_" + count + "'";
            if (itemdata.KeepStock == true) {
                var qntmin = 0;
                if (itemdata.ItemUnit == itemdata.ItemUnitID) {
                    qntmin = Quantity * itemdata.ConFactor;
                }
                if (itemdata.ItemUnit == itemdata.SubUnitId) {
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
            //---------
            var bqty = (itemdata.bqty == null) ? Quantity : itemdata.bqty;
        }
        data = "<td class='text-center'> " + slno + " </td>" +
               "<td style='width:550px;' class='input-group input-group-sm'><select data-name='ItemId' class='form-control item_name' " + required + " data-id='" + count + "' placeholder='Item Name' id='item_name_" + count + "'  data-val-required='The Item field is required' readonly='readonly'>" + Option + "</select></td>" +
               "<td style='width:80px;'><select data-name='Unit' class='form-control units unit_name_" + count + "' id='unit_name_" + count + "' " + required + " data-id='" + count + "' id='unit_name' tabindex='" + tab2 + "' readonly='readonly'></select></td>" +
               "<td> <input type='text' data-name='Quantity' name='product_quantity[]' data-msg-min ='The Item Quantity must be Greater than Zero' id='total_qnt_" + count + "' value='" + Quantity + "'  class='total_qnt_" + count + " form-control text-right qty' placeholder='0' value='0' min='.01' tabindex='" + tab3 + "' readonly='readonly'/><input type='hidden' class='baseqty' name='baseqty' id='baseqty_" + generatedcount + "' value='" + bqty + "'/></td>" +
               "<td> <input type='text' data-name='PPrice' id='total_price_" + count + "' value='" + Price.toFixed(2) + "'  class='total_price_" + count + " form-control text-right price' placeholder='0' value='0' tabindex='" + tab4 + "' readonly='readonly'/></td>" +
               "<td> <input type='text' data-name='PAmount' id='total_amount_" + count + "' value='" + Amount.toFixed(2) + "'  class='total_amount_" + count + " form-control text-right consume_amt' placeholder='0' value='0' tabindex='" + tab5 + "' readonly='readonly'/><input type='hidden' class='bomid' data-name='BOM' name='bomid' id='bomid_" + count + "' value='" + Bomid + "'/>" + htdata + "</td>";
        row += data + "</tr>";

        $('#' + t).append(row);
        // searchbomItem();
        if (itemdata) {
            createUnitList(itemdata, count);
        }
        count++;
        CalculateTotal();
    }
}

function bomgenerated(t, action, bom, itemdata, price) {//Quantity, Unit, ItemId, ItemWithCode, Expense, Price, Bomid, BOMName) {//t, action, ItemUnit, ItemQuantity, Item, ItemCode, ItemName, ItemWithCode, ItemNote, itemdata, type, BaseQty, ItemDiscount, pkt) {
   

    if (generatedcount == limits) alert("You have reached the limit of adding " + generatedcount + " inputs");
    else {
        var Option = "";
        var optionunit = "";
        var required = "";
        var slno = $('#addbomgenerated tr').length + 1;
        var a = "gitem_name" + generatedcount,
        tabindex = generatedcount * 5;
        var row = "<tr class='gitem_' id='gitem_" + generatedcount + "'>";
        var data = "";
        var htdata = "";
        var divid = "gitem_name_" + bom.BOMId;
        var itselect = "";
        var Price = 0;
        var Amount = 0;
        var Expense = 0;
        if (bom.ItemId != null) {
            row = "<tr class='gitem_" + bom.ItemId + "' id='gitem_" + generatedcount + "'>";
            Option = "<option value='" + bom.ItemId + "'>" + bom.ItemNamewithcode + "</option>"; 
            OptBOM = "<option value='" + bom.BOMId + "'>" + bom.BOMName + "</option>";
            itselect = "<select class='form-control' " + required + " data-id='" + generatedcount + "' id='gitem_name_" + generatedcount + "'>" + Option + "</select>"
        } else {
            itselect = "<select class='form-control gitem_name' " + required + " data-id='" + generatedcount + "' placeholder='Item Name' id='gitem_name_" + generatedcount + "'  data-msg-required='The Item Name is required' onchange='GetItemdetails(this," + generatedcount + ",\"" + type + "\")'>" + Option + "</select>"
        }
        if (generatedcount == 1) {
            required = 'required="required"';
        }
        var inote = "";
        var readonly = "";
        var readonlyqty = "";
        var deletebtn = "<button style='text-align: right;' class='btn btn-danger' type='button' value='Delete' onclick='deleteRow(this," + bom.ItemId + ")'><i class='fa fa-trash fa-1x'></i></button> ";
        if (bom) {
            if (bom.Unit == itemdata.ItemUnitID) {
                Price = (itemdata.price);
            }
            if (bom.Unit == itemdata.SubUnitId) {
                Price = ((itemdata.price) / itemdata.ConFactor);
            }
            Amount = (bom.Quantity * Price);

            //minimum stock check
            htdata = "<div class='mstock minstock_" + generatedcount + "'";
            if (itemdata.KeepStock == true) {
                var qntmin = 0;
                if (itemdata.ItemUnit == itemdata.ItemUnitID) {
                    qntmin = bom.Quantity * itemdata.ConFactor;
                }
                if (itemdata.ItemUnit == itemdata.SubUnitId) {
                    qntmin = bom.Quantity;
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
            //---------
        }


        //Item = (typeof Item == 'undefined') ? 0 : Item;
        //price = (bom.Price == 0) ? price : bom.Price;
        Amount = (bom.Amount == 0) ? price : bom.Amount;

        var ref="direct";
        data = "<td class='text-center'> " + slno + " </td>" +
                       "<td style='width:550px;' class='input-group input-group-sm'><select data-name='ItemId' class='form-control item_consumed' " + required + " data-id='" + generatedcount + "' placeholder='Item Name' id='gitem_name_" + generatedcount + "'  data-val-required='The Item field is required' readonly='readonly'>" + Option + "</select></td>" +
                       "<td style='width:150px;'><select data-name='ItemUnit' class='form-control units unit_name_" + generatedcount + "' id='unit_name_" + generatedcount + "' " + required + " data-id='" + generatedcount + "' readonly='readonly'></select></td>" +
                       "<td> <input type='text' data-name='Quantity' name='product_quantity[]' onchange='qty_change(" + generatedcount +"," +bom.BOMId + ")' data-msg-min ='The Item Quantity must be Greater than Zero' id='ctotal_qntt_" + generatedcount + "' value='" + bom.Quantity + "'  class='total_qntt_" + generatedcount + " form-control text-right quty' placeholder='0' value='0' min='.01' /></td>" +
                       "<td> <input type='text' data-name='Expense' id='Expense" + generatedcount + "' onchange='xpns_change(" + generatedcount +"," +bom.BOMId + ")' value='" + bom.Expense + "'  class='total_expense_" + generatedcount + " form-control text-right gen_expense' placeholder='0' value='0'/></td>" +
                       "<td> <input type='text' data-name='Price' id='ctotal_price_" + generatedcount + "' value='" + price + "' onchange='xpns_change(" + generatedcount + "," + bom.BOMId + ")'  class='total_price_" + generatedcount + " form-control text-right gen_price' placeholder='0' value='0'/></td>" +
                       "<td> <input type='text' data-name='Amount' id='ctotal_amount_" + generatedcount + "' value='" + Amount + "'  class='total_amount_" + generatedcount + " form-control text-right gen_amt' placeholder='0' value='0' readonly='readonly'/><input type='hidden' class='cqty_give' name='cqty_give' id='cqty_give_" + generatedcount + "' value='" + bom.Quantity + "'/><input type='hidden' class='cbomid' data-name='BOMId' name='cbomid' id='cbomid_" + generatedcount + "' value='" + bom.BOMId + "'/>" + htdata + "</td>";
        row += data + "</tr>";

        $('#' + t).append(row);
        //searchItem();
        var qty = $("#GenItemQty").val();
        var CurQty = bom.Quantity;
        var tot = parseFloat(qty) + parseFloat(CurQty);
        //alert(qty + ' t ' + CurQty + ' t ' + tot);
        $("#GenItemQty").val(tot);
        if (bom) {
            createUnitList(bom, generatedcount);
        }
        if (action == "edit") {
            CalculateTotal();
            $("#actionvalue").val(action);
        } else {
            $("#actionvalue").val("");
        }

        //$("#item_name_" + count).css("width", "550px");

        generatedcount++;
    }
}
function funbom(t, action, BOMName, BOMId, BOMNamewithcode) {
    if (bcount == limits) alert("You have reached the limit of adding " + bcount + " inputs");
    else {
        var Option = "";
        var optionunit = "";
        var required = "";
        var slno = $('#bombody tr').length + 1;
        var a = "item_bom" + bcount,
        tabindex = bcount * 5;
        var row = "<tr class='bom_' id='bom_" + bcount + "'>";
        var data = "";
        var htdata = "";
        var divid = "item_bom_" + BOMId;
        var itselect = "";
        var Price = 0;
        var Expense = 0;
        if (BOMId != null) {
            row = "<tr class='bom_" + BOMId + "' id='" + bcount + "'>";
            Option = "<option value='" + BOMId + "'>" + BOMNamewithcode + "</option>";
            itselect = "<select class='form-control' " + required + " data-id='" + bcount + "' id='item_bom_" + bcount + "'>" + Option + "</select>"
        } else {
            itselect = "<select class='form-control item_bom' " + required + " data-id='" + bcount + "' placeholder='Item Name' id='item_bom_" + bcount + "'  data-msg-required='The Item Name is required' onchange='GetBOM(this," + bcount + ")'>" + Option + "</select>"
        }
        if (bcount == 1) {
            required = 'required="required"';
        }
        var inote = "";
        var readonly = "";
        var readonlyqty = "";
        var deletebtn = "<button style='text-align: right;' class='btn btn-danger' type='button' value='Delete' onclick='deleteRow(this," + BOMId + ")'><i class='fa fa-trash fa-1x'></i></button> ";

        //Item = (typeof Item == 'undefined') ? 0 : Item;
        data = "<td class='text-center'> " + slno + " </td>" +
                       "<td style='width:400px;' class='input-group input-group-sm'><select data-name='BOMName' class='form-control item_bom' data-id='" + bcount + "' placeholder='Bill Of Material' id='item_bom'  data-val-required='The BOM name field is required' onchange='GetBOM(this," + bcount + ")'>"
                       + Option + "</select><input type='hidden' data-name='BOM_Id' class='bbomid' name='bomid' id='bbomid_" + bcount + "' value='" + BOMId + "'/></td>" +
                       "<td style='width:10px;' class='text-center'><button style='text-align: right;' class='btn btn-danger' type='button' value='Delete' onclick='deleteRow(this," + bcount + ")'><i class='fa fa-trash fa-1x'></i></button> </td>";
        row += data + "</tr>";

        $('#' + t).append(row);
        searchItem();

        if (action == "edit") {
            $("#actionvalue").val(action);
        } else {
            $("#actionvalue").val("");
        }

        //$("#item_name_" + count).css("width", "550px");

        bcount++;
    }
}

function GetItemdetails(selectObject) {
    $("#total_qntt_" + dataid).attr('readonly', false);
    $("#item_discount" + dataid).attr('readonly', false);
    $("#price_item_" + dataid).attr('readonly', false);

    
    if (selectObject.value) {
        var ItemId = selectObject.value;
        if (ItemId != null) {
            if ($(".bom_" + ItemId).length > 0) {
                if ($(".bom_" + ItemId).length < 10) {
                    if (confirm('Are you sure want to Add this item Again?')) {
                        itemUpdate(selectObject, dataid, action);
                        if ($(".bom_").length == 0) {
                            funbom('bombody', '', '0');
                        }
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
                if ($(".item_bom").length == 0) {
                    funbom('bombody', '', '0');
                }
            }
        }
    }
}

function searchItem() {
    var selecteditem = new Array();
    $(".item_bom").each(function () {
        selecteditem.push($(this).val());
    });

    $(".item_bom").select2({
        placeholder: 'Search Bill of Material ',
        minimumInputLength: 0,
        ajax: {
            url: "/BOM/SearchBOM",
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

function GetBOM(selectObject, dataid) {
    if (selectObject.value) {
        var ItemId = selectObject.value;
        if (ItemId != null) {
            if ($(".bom_" + ItemId).length > 0) {
                if ($(".bom_" + ItemId).length < 10) {
                    if (confirm('Are you sure want to Add this item Again?')) {
                        var type = "add";
                        bomUpdate(selectObject, dataid, type);
                        //$(selectObject).val(null).trigger('change');
                        searchItem();
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
                bomUpdate(selectObject, dataid);
                searchItem();
            }
        }
    }
}

function bomUpdate(selectObject, dataid,type) {

    var BomID = selectObject.value; 
    if (type == "add")
    {
        var tbody = $("#bomgenerated tbody");
        if (tbody.children().length > 0) {
            tbody.children("tr").each(function () {
                var rowid = $(this).attr("id");
                var bomid = $("#" + rowid + " .cbomid").val();
                if (BomID == bomid) {
                    //Qty
                    var qty = $("#" + rowid + " .quty").val();
                    var subtotal = parseFloat(qty) + 1;
                    $("#" + rowid + " .quty").val(subtotal);
                    qty_change(rowid,bomid,"againadd");
                    //$(this).closest("tr").remove();
                }
            });
        }
        $('#bom tr:last').remove();
        funbom('bombody', '', '0');
    }
    else {
        $.ajax({
            url: '/BOM/GetBOMDetails',
            dataType: 'json',
            type: "POST",
            data: { BomID: BomID },
            cache: true,
            async: false,
            success: function (data) {
                var newOption = $('<option></option>');
                newOption.val(data.bom.ItemId).html(data.bom.ItemName);
                $('#ddlItem').html('');
                $("#Productioncost").val(data.bom.prductioncost);
                $("#Labourcost").val(data.bom.labourcost);
                $("#meterialcost").val(data.bom.materialcost);
                $('#ddlItem').append(newOption);
                $(selectObject).closest('tr').attr('class', "bom_" + data.bom.BOMId);
                $("#PriceVal").text('');
                $("#bbomid_" + dataid).val(data.bom.BOMId);
                var price = 0;
                $.each(data.item, function (i, item) {
                    bomitem('addbomitem', '', item.Quantity, item.Unit, item.ItemId, item.ItemWithCode, item, item.BOMId);
                    price = price + (item.Quantity * item.price);
                });

                bomgenerated('addbomgenerated', '', data.bom, data.item, price);
                $("#PriceVal").text(price);
                CalculateTotal();
                $("#tablediv").show();
                if ($(".item_").length == 0) {
                    funbom('bombody', '', '0');
                }
            }
        });
    }
}
//Qty function
function qty_change(dataid,bomid,type) {
    var quantity ;//= parseFloat($("#" + dataid + " .quty").val());//parseFloat($(".total_qntt_" + dataid).val());
    checkminstock(bomid);
    var ItemBomid;
    var tbodygenerated = $("#bomgenerated tbody");
    if (tbodygenerated.children().length > 0) {
        tbodygenerated.children("tr").each(function () {
            var rowid = $(this).attr("id");
            ItemBomid = $("#" + rowid + " .cbomid").val();
            if (bomid == ItemBomid)
            {
                //Price          
                quantity = $("#" + rowid + " .quty").val();
                var rate = $("#" + rowid + " .gen_price").val();
                //alert(rate)
                var xpns = parseFloat($("#" + rowid + " .gen_expense").val());
                var subtotal = (quantity * rate) + xpns;
                $("#" + rowid + " .gen_amt").val(subtotal.toFixed(2));
            }
            if (rowid==dataid && type == "againadd")
            {             
                var newqty=(quantity) + 1;
                //Price
                var Genqty = $("#" + rowid + " .quty").val();
                var rate = $("#" + rowid + " .gen_price").val();
                var subtotal = Genqty * rate;
                $("#" + rowid + " .gen_amt").val(subtotal.toFixed(2));
                $("#" + rowid + " .quty").val(Genqty);
            }
       
            var gtotal = 0;
            var tbody = $("#bomitem tbody");
            if (tbody.children().length > 0) {
                tbody.children("tr").each(function () {
                    var Itemrowid = $(this).attr("id");
            
                    var ItemBom = $("#" + Itemrowid + " .bomid").val();
                    if (bomid == ItemBom) {
                        //Qty
                        var bqty = $("#" + Itemrowid + " .baseqty").val();
                        var NewQty = bqty * quantity;
                        $("#" + Itemrowid + " .qty").val(NewQty.toFixed(2));
                        //Expense
                        var rate = $("#" + Itemrowid + " .price").val();                
                        var subtotal = (NewQty * rate);
                         gtotal += subtotal;
                        //alert(NewQty + ' ' + rate + ' ' + xpns + ' ' + subtotal)
                        $("#" + Itemrowid + " .consume_amt").val(subtotal.toFixed(2));
                    }
                });
            }
            if (bomid == ItemBomid) {
               // $("#" + rowid + " .gen_price").val(gtotal.toFixed(2));
                var cqty = parseFloat($("#" + rowid + " .quty").val());
                var cgtotal = parseFloat($("#" + rowid + " .gen_price").val());
                var xpns = parseFloat($("#" + rowid + " .gen_expense").val());
                var ctotal = (cqty * cgtotal) + xpns;
                $("#" + rowid + " .gen_amt").val(ctotal.toFixed(2));
            }
        });
    }

    //var tbodygenerated = $("#bomgenerated tbody");
    //if (tbodygenerated.children().length > 0) {
    //    tbodygenerated.children("tr").each(function () {
    //        var rowid = $(this).attr("id");
    //        ItemBomid = $("#" + rowid + " .cbomid").val();
    //        if (bomid == ItemBomid)
    //        {

    //        }
    //    });
    //}

    //var price = parseFloat($("#total_price_" + dataid).val());
    //price = price * quantity;
    //$("#total_amount_" + dataid).val(price);


    CalculateTotal();

}

//Xpns function
function xpns_change(dataid, bomid, type) {
    var quantity;//= parseFloat($("#" + dataid + " .quty").val());//parseFloat($(".total_qntt_" + dataid).val());
    checkminstock(bomid);
    var ItemBomid;
    var tbodygenerated = $("#bomgenerated tbody");
    if (tbodygenerated.children().length > 0) {
        tbodygenerated.children("tr").each(function () {
            var rowid = $(this).attr("id");
            ItemBomid = $("#" + rowid + " .cbomid").val();
            if (bomid == ItemBomid) {
                //Price          
                quantity = parseFloat($("#" + rowid + " .quty").val());
                var rate = parseFloat($("#" + rowid + " .gen_price").val());
                var xpns = parseFloat($("#" + rowid + " .gen_expense").val());
                quantity = quantity || 0;
                rate = rate || 0;
                xpns = xpns || 0;
                var subtotal = (quantity * rate) + xpns;
                //alert(quantity + ' ' + rate + ' ' + xpns + ' ' + subtotal)
                $("#" + rowid + " .gen_amt").val(parseFloat(subtotal).toFixed(2));
            }
        });
    }   

    var price = parseFloat($("#total_price_" + dataid).val());
    price = price * quantity;
    $("#total_amount_" + dataid).val(price.toFixed(2));
    CalculateTotal();

}
//delete function
function deleteRow(t, dataid) {
    var bm = $("#bbomid_" + dataid).val(); //$(t).closest('tr').attr('class');//

    var bid = $("#" + bm + ".item_bom").val();
    var classname = $(t).closest('tr').attr('class');
    if (classname == 'bom_') alert("Sorry you can't delete this row.");
    else {
        var r = confirm("Are you sure you want to delete this..?");
        if (r == true) {
            var classname = $(t).closest('tr').attr('class');

            //delete item
            var tbodyitem = $("#bomitem tbody");
            if (tbodyitem.children().length > 0) {
                tbodyitem.children("tr").each(function () {
                    var rowid = $(this).attr("id");
                    var bomid = $("#" + rowid + " .bomid").val();

                    var subitem = bid;
                    if (bm == bomid) {
                        $(this).closest("tr").remove();
                    }
                });
            }

            //delete consumed
            var tbody = $("#bomgenerated tbody");
            if (tbody.children().length > 0) {
                tbody.children("tr").each(function () {
                    var rowid = $(this).attr("id");
                    var bomid = $("#" + rowid + " .cbomid").val();
                    var subitem = bid;
                    if (bm == bomid) {
                        $(this).closest("tr").remove();
                    }
                });
            }
            //delete bom
            var e = t.parentNode.parentNode;
            e.parentNode.removeChild(e);

            var leng = $('#addbomgenerated tr').length;
            if (leng == 0) {

                $('#addbomitem').html('');
                $('#addbomgenerated').html('');
                $("#tablediv").hide();
            }
            var i = 1;
            $('#bombody tr').each(function () {
                $(this).find('td:first').text(i);
                i++;
            });
            var j = 1;
            $('#addbomitem tr').each(function () {
                $(this).find('td:first').text(j);
                j++;
            });
            var k = 1;
            $('#addbomgenerated tr').each(function () {
                $(this).find('td:first').text(k);
                k++;
            });
        }
    }
}
// create units based on primary and secondary
function createUnitList(result, dataid) {
    // clear previous content
    $('#unit_name_' + dataid).empty();
    var newOption = $('<option></option>');
    if (result.Unit != null) {
        if (result.Unit == result.ItemUnitID) {
            newOption.val(result.Unit).html(result.PriUnit);
        }
        if (result.Unit == result.SubUnitId) {
            newOption.val(result.Unit).html(result.SubUnit);
        }
        $('#unit_name_' + dataid).append(newOption);
    }
}


function qtychange() {
    var tbody = $("#bomitem tbody");
    if (tbody.children().length > 0) {
        tbody.children("tr").each(function () {
            var rowid = $(this).attr("id");
            var Qty = $("#" + rowid + " .qty_give").val();

            var EnterQty = $("#Qty").val();
            var HQty = $("#QtyVal").val();
            var newQty = parseFloat(Qty * (EnterQty / HQty)).toFixed(2);
            $("#" + rowid + " .quty").val(newQty);


            var quantity = $("#" + rowid + " .quty").val();
            var rate = $("#" + rowid + " .price").val();
            var subtotal = quantity * rate;
            $("#" + rowid + " .amt").val(subtotal.toFixed(2));

        });

        CalculateTotal();
    }
}

function CalculateTotal() {

    var qty = $(".quty").val();
    if (qty != 0) {
        
        var tbody = $("#bomitem tbody");
        if (tbody.children().length > 0) {
            var gtSubTotal = 0;
            var gtQty = 0;
            var gtTotal = 0;

            $(".quty").each(function () {
                var subQty = this.value;
                gtQty = parseFloat(gtQty) + parseFloat(subQty);
            });
            $(".gen_amt").each(function () {
                var subTot = this.value;
                gtSubTotal = parseFloat(gtSubTotal) + parseFloat(subTot);
            });

            gtSubTotal = gtSubTotal || 0.00;
         

          
            $("#ItemQty").text(gtQty.toFixed(2));
            $("#Total").text(gtSubTotal.toFixed(2));
        }
    }

    var total = $("#Total").text();
    var exp = $("#Expense").val();
    var qty = $("#Qty").val();

    var totPrice = (parseFloat(total) + ((parseFloat(exp)).toFixed(2) * (parseFloat(qty)).toFixed(2)));

    totPrice = totPrice || 0.00;

    $("#Amount").val((totPrice).toFixed(2));
    var price = (totPrice / qty);

    price = price || 0.00;

    $("#Price").val((price).toFixed(2));
}
function ProdSubmit(fnval) {

    if (checkminstock()) {

        var HTMLtblbom = {
            getData: function (table) {
                var data = [];
                table.find('tr').not(':first').not('.bom_').each(function (rowIndex, r) {
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
        var GHTMLtbl = {
            getData: function (table) {
                var data = [];
                table.find('tr').not(':first').not('.gitem_').each(function (rowIndex, r) {
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

        var pdata = {
            'VoucherNo': $('#VoucherNo').val(),
            'PEDate': $('#PEDate').val(),
            'Note': $('#Note').val(),
            'Branch': $('#ddlBranch').val(),
            'MaterialCenter': $('#ddlMC').val(),
            'fnval': $('#fnval').val(),
             'Productioncost' : $("#Productioncost").val(),
             'Labourcost' : $("#Labourcost").val(),
             'meterialcost' : $("#meterialcost").val(),
        }
        var data = HTMLtbl.getData($('#bomitem'));
        var datagenerated = GHTMLtbl.getData($('#bomgenerated'));
        var databom = HTMLtblbom.getData($('#bom'));
        var parameters = {};
        parameters.prodata = pdata;
        parameters.proitem = data;
        parameters.progenerated = datagenerated;
        parameters.bom = databom;
        parameters.PEDate = $('#PEDate').val();
        parameters.ApprovedBy = $('#SelApprovedBy').val();
        parameters.fnval = fnval;

        parameters.Ref1 = $('#Ref1').val();
        parameters.Ref2 = $('#Ref2').val();
        parameters.Ref3 = $('#Ref3').val();
        parameters.Ref4 = $('#Ref4').val();
        parameters.Ref5 = $('#Ref5').val();

        parameters.Project = $('#ddlProject').val();
        parameters.ProTask = $('#ddlProTask').val();

        var url = "";
        if (fnval == "save" || fnval == "print") {
            url = $('#createForm')[0].action;
        }
        if (fnval == "update" || fnval == "updateandprint") {
            url = $('#updateForm')[0].action;
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
                if (e.status) {
                    $('.ajax_response', res_success).text(e.message);
                    $('.AlertDiv').prepend(res_success);
                    if (fnval == "update") {
                        window.location.href = '/Production/Index';
                    }
                    if (fnval == "save") {
                        window.location.href = '/Production/Create';
                    }
                    if (fnval == "print" || fnval == "updateandprint") {
                        prodprint(e, e.fmapp);
                    }
                    if (fnval == "update" || fnval == "updateandprint")
                    {
                        setTimeout(function () { location.reload(); });
                    }
                    else {
                        setTimeout(function () { window.location.href = '/Production/Create'; });
                    }
                    
                }
                else {
                    $('.ajax_response', res_danger).text(e.message);
                    $('.AlertDiv').prepend(res_danger);
                    $("button").prop('disabled', false); // enable button
                }
            }
        });
    }
}
function checkminstock(bomid) {

    var outofstock = 0;
    var outitems = "";
    var tbody = $("#bomitem tbody");
    if (tbody.children().length > 0) {
        tbody.children("tr").each(function () {
            var rwid = $(this).attr("id");
            var itembomid = $("#" + rwid + " .bomid").val();
            if (bomid == itembomid)
            {
                var keepstock = $("#" + rwid + " .mstock").attr('data-keeps');
                if (keepstock == "yes") {
                    var index = $("#" + rwid + " .units").prop('selectedIndex');
                    var unitname = $("#" + rwid + " .units").find('option:selected').text();
                    var minstock = parseFloat($("#" + rwid + " .mstock").attr('data-min'));
                    var confactor = parseFloat($("#" + rwid + " .mstock").attr('data-confactor'));
                    var stock = parseFloat($("#" + rwid + " .mstock").attr('data-stock'));
                    var quantity = parseFloat($("#" + rwid + " .qty").val());
                    var itemname = $("#" + rwid + " .item_name").text();
                    //alert(index + ' t ' + unitname+' t '+minstock+' t '+confactor+' t '+stock+' t '+quantity+' t '+itemname)
                    var qty = 0;
                    var classn = $("#" + rwid).attr('class');
                    //alert(classn);
                    $("." + classn).each(function () {
                        var rowid = $(this).attr('id');
                        var arr = rowid.split('_');
                        // var arg1 = arr[1];
                        var index1 = $("#" + rowid + " .units").prop('selectedIndex');
                        var curent = $("#" + rowid + " .qty").val();
                        var confactor1 = parseFloat($("#" + rowid + "  .mstock").attr('data-confactor'));
                        if (index == 0) {
                            qty += (curent * confactor1);
                        }
                        else {
                            qty += curent;
                        }
                        //alert(index + ' t ' + rowid + ' t ' + arr + ' t ' + index1 + ' t ' + curent + ' t ' + confactor1 + ' t ' + qty)
                    });

                    //var qtyunit =  "Only "+stock + " " + unitname +"Items Are Available In Stock..";
                    //itemname = itemname + qtyunit;
                    var ItemOutOfStock = $("#ItemOutOfStock").val();

                    if (index == 0) {
                        stock = stock - (qty - quantity);
                        minstock = minstock / confactor;
                        stock = stock / confactor;
                        var tostock = stock - quantity;
                        var totstock = tostock / confactor;
                        if (totstock < 0 && ItemOutOfStock == 'inactive') {
                            outofstock++;
                            stock = stock.toFixed(2);
                            alert("This Item("+itemname+") Is Going To Out of Stock!!! Only " + stock + " " + unitname + "Items Are Available In Stock..");
                            outitems = outitems + ", " + itemname;
                        }
                    } else {
                        stock = stock - (qty - quantity);
                        var totstock = stock - quantity;
                        if (totstock < 0 && ItemOutOfStock == 'inactive') {
                            outofstock++;
                            outitems = outitems + ", " + itemname;
                            alert("This Item(" + itemname + ") Is Going To Out of Stock!!! Only " + stock + " " + unitname + " Items Are Available In Stock..");

                        }
                    }
                }
            }
           

        });
    }
    //alert(outofstock)
    if (outofstock = 0) {
        alert("These Item Is Going To Out of Stock!!! " + outitems);
        //return false;
        return true;
    }
    else if (outofstock<0) {
        alert("These Item Is Out of Stock!!! " + outitems);
        return false;
    }
    else {
        return true;
    }

}

function prodprint(e,fmapp) {
    var dat = convertToDate(e.Data.PEDate);
    $("#lblBillNo").text(e.prodata.VoucherNo);
    $("#lblDate").text(dat);

    $("#lblItemQty").text(e.arr[0]);
    $("#lblTotal").text(e.arr[1]);

    $("#lblQty").text(e.arr[2]);
    $("#lblItemTotal").text(e.arr[3]);

    $("#lblBranch").text(e.Data.BranchName);
    $("#lblMC").text(e.Data.MCName);
    (e.BOM.BOMName);

     if (e.ComHeadCheck == 0) {

        $("#ComHeadCheck").hide();
    }
    else {
        $("#ComHeadCheck").show();
    }

     if (e.Data.Project != null) {
         $("#lblProject").text(e.Data.ProjectName);
         $(".pro").show();
     } else {
         $(".pro").hide();
     }

     if (e.Data.ProTask != null) {
         $("#lblTask").text(e.Data.TaskName);
         $(".tsk").show();
     } else {
         $(".tsk").hide();
     }

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


    var lblbom = $('#lblBOM').text('');
    $.each(e.BOM, function (i, item) {
        var str = "";
        str += item.BOMName + ', ';
        lblbom.append(str);
    });
    var count = 1;
    $.each(e.progen, function (i, item) {
        var unit = ((item.Unit != null) ? item.Unit : "");
        var str = '<tr>';
        str += '<td>' + count + '</td>';
        str += '<td>' + item.Item + '</td>';
         str += '<td>' + unit + '</td>';
        str += '<td>' + item.Qty + '</td>';       
        //str += '<td>' + item.Expense + '</td>';
        str += '<td>' + item.Price + '</td>';
        str += '<td>' + item.Amount + '</td>';
        str += '</tr>';
        count++;
        $('#itemtablecon').append(str);
    });
    var countIt = 1;
    $.each(e.proitems, function (i, item) {
        var unit = ((item.Unit != null) ? item.Unit : "");
        var str = '<tr>';
        str += '<td>' + countIt + '</td>';
        str += '<td>' + item.Item + '</td>';
        str += '<td>' + unit + '</td>';
        str += '<td>' + item.Qty + '</td>';
        str += '<td>' + item.Price + '</td>';
        str += '<td>' + item.Amount + '</td>';
        str += '</tr>';
        countIt++;
        $('#itemtableitem').append(str);
    });

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

    var originalpage = document.body.innerHTML;
    var printContent = $('#printit').html();
    $('title').html("Production Voucher - " + e.prodata.VoucherNo);
    $('body').html(printContent);
    window.print();
}
