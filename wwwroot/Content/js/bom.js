var count = 1, type = '';
limits = 500;
function addbomitem(t, action, Quantity, Unit, Item, ItemWithCode, itemdata) {
    if (count == limits) alert("You have reached the limit of adding " + count + " inputs");
    else {
        var data = "";
        var Type = "";
        var Option = "";
        var readonly = "";
        var row = "<tr class='item_' id='item_" + count + "'>";
        var slno = $('#addbomitem tr').length + 1;
        var a = "item_name" + count,
        tabindex = count * 4;
        tab1 = tabindex + 1;
        tab2 = tabindex + 2;
        tab3 = tabindex + 3;
        tab4 = tabindex + 4;
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
        var itemaddbtn = "<span class='input-group-btn'><a type='button' href='/Item/AddItem' class='modal-create-lg btn btn-success btn-flat'><i class='fa fa-1x fa-plus-circle'></i></a></span>";

        data = "<td class='text-center'> " + slno + " </td>" +
               "<td class='input-group input-group-sm'><select data-name='ItemId' class='form-control item_name' " + required + " data-id='" + count + "' placeholder='Item Name' id='item_name_" + count + "'  data-val-required='The Item field is required' onchange='GetItemdetails(this," + count + ",\"" + type + "\")'>" + Option + "</select> " + itemaddbtn + "</td>" +
               "<td style='width:100px;'><select data-name='Unit' class='form-control units unit_name_" + count + "' id='unit_name_" + count + "' " + required + " data-id='" + count + "' id='unit_name' tabindex='" + tab2 + "'></select></td>" +
               "<td> <input type='number' data-name='Quantity' name='product_quantity[]' onchange='quantity_change(" + count + ");' id='total_qntt_" + count + "' value='" + Quantity + "'  class='total_qntt_" + count + " form-control text-right quty' placeholder='0' value='0' min='.01' tabindex='" + tab3 + "'/></td>" +
            "<td> <input type='number' data-name='purchaseprice' name='purchaseprice_quantity[]' onchange='rate_change(" + count + ");'  id='total_purchase_" + count + "'   class='total_purchase_" + count + " form-control text-right purchase' placeholder='0' value='0' min='.01' tabindex='" + tab3 + "'/></td>" +
            "<td> <input type='number' data-name='tpurchaseprice' name='tpurchaseprice_quantity[]'  id='ttotal_purchase_" + count + "'   class='ttotal_purchase_" + count + " form-control text-right tpurchase' placeholder='0' value='0' min='.01' tabindex='" + tab3 + "'/></td>" +

            "<td class='text-center'><button tabindex='" + tab4 + "' style='text-align: right;' class='btn btn-danger' type='button' value='Delete' onclick='deleteRow(this)'><i class='fa fa-trash fa-1x'></i></button></td>";
        row += data + "</tr>";

        $('#' + t).append(row);
        
        var date = $("#BOMDate").val();
        var MC = $("#ddlMC").val();
        searchbomItem(date,MC);
        if (itemdata) {
            createUnitList(itemdata, count);
            var purprice = parseFloat(itemdata.price);
            $("#total_purchase_" + count).val(itemdata.price);
            $("#ttotal_purchase_" + count).val(parseFloat(purprice * Quantity).toFixed(2));
        }
        count++;
        setTabIndex();
    }
}
function quantity_change(arg) {
    var qty = $('#total_qntt_' + arg).val();
    var price = $('#total_purchase_' + arg).val();
    var grandtotal = parseFloat(qty * price);
    $('#ttotal_purchase_' + arg).val(grandtotal);
    CalculateTotal();
}
function rate_change(arg) {
    
    var qty = $('#total_qntt_' + arg).val();
    var price = $('#total_purchase_' + arg).val();
    var grandtotal = parseFloat(qty * price);
    $('#ttotal_purchase_' + arg).val(grandtotal);
    CalculateTotal();
}
function searchbomItem(date, MC) {
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
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0,
                    x: "All"
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

    });
}


function CalculateTotal() {

    var qty = $(".quty").val();
    if (qty != 0) {

        var tbody = $("#bomtable tbody");
        if (tbody.children().length > 0) {
            var gtSubTotal = 0;
            var gtQty = 0;
            var gtTotal = 0;

     
            $(".tpurchase").each(function () {
                var subTot = this.value;
                gtSubTotal = parseFloat(gtSubTotal) + parseFloat(subTot);
            });

            gtSubTotal = gtSubTotal || 0.00;



            var productioncost = $("#Expense").val();
            var labourcost = $("#Labourcost").val();
            $("#meterialcost").val(gtSubTotal);

            var totPrice = parseFloat(gtSubTotal) + parseFloat(productioncost) + parseFloat(labourcost);// + parseFloat(materialcost);

            $("#Total2").text(totPrice.toFixed(2));
        }
    }

 
    totPrice = totPrice || 0.00;

    $("#Amount").val((totPrice).toFixed(2));
    var price = (totPrice / qty);

    price = price || 0.00;

    $("#Price").val((price).toFixed(2));
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
//item details
function GetItemdetails(selectObject, dataid, action) {
    if (selectObject.value) {
        var ItemId = selectObject.value;
        if (ItemId != null) {
            if ($(".item_" + ItemId).length > 0) {
                if (confirm('Are you sure want to Add this item Again?')) {
                    itemUpdate(selectObject, dataid, action);
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
    $.ajax({
        url: '/Item/GetItem',
        type: "GET",
        dataType: "JSON",
        data: { itemID: selectObject.value },
        success: function (result) {
              createUnitList(result, dataid);
            $("#total_qntt_" + dataid).val(1);
            $("#total_purchase_" + dataid).val(result.PurchasePrice);
            $("#ttotal_purchase_" + dataid).val(result.PurchasePrice);
                $(selectObject).closest('tr').attr('class', "item_" + result.ItemID);
                if ($(".item_").length == 0) {
                    addbomitem('addbomitem', '','0');
                }
            $('.unit_name_' + dataid).focus();
            CalculateTotal();
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
            if (result.Unit) {
                if (result.Unit == result.ItemUnitID)
                    newOption.attr("selected", "selected");
                if (result.Unit == result.SubUnitId)
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
}

function BOMSubmit(fnval) {
    var HTMLtbl = {
        getData: function (table) {
            var data = [];
                table.find('tr').not(':first').not('.item_').each(function (rowIndex, r) {
                var cols = {};
                $(this).find('input,textarea,select').each(function (colIndex, c) {
                    itid = $(this).attr('data-name').split(' ')[0];
                    itval = ($(this).val() != "") ? $(this).val() : $(this).text();
                    cols[itid] = itval;
                });
                data.push(cols);
            });
            return data;
        }
    }

    var bomdata = {
        'BOMId': $('#BOMId').val(),
        'BOMName': $('#BOMName').val(),
        'ItemId': $('#ddlItem').val(),
        'Quantity': $('#Quantity').val(),
        'Unit': $('#Unit').val(),
        'Expense': $('#Expense').val(),
        'Labourcost': $('#Labourcost').val(),
        'meterialcost': $('#meterialcost').val(),
        'Account': $('#ddlAccount').val(),
        'Branch': $('#ddlBranch').val(),
        'MaterialCenter': $('#ddlMC').val(),
    }
    var data = HTMLtbl.getData($('#bomtable'));
    var parameters = {};
    parameters.bomdata = bomdata;
    parameters.bomitems = data;
    parameters.BOMDate = $('#BOMDate').val();

    var url = "";
    if (fnval == "save") {
        url = $('#createForm')[0].action;
    }
    if (fnval == "update") {
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
            $('.ajax_response', res_success).text(e.message);
            $('.AlertDiv').prepend(res_success);
            if (fnval != null) {
                window.location.href = '/BOM/Index';
            } else {
                location.reload();
            }
            $("button").prop('disabled', false); // enable button
        }
    });
}
