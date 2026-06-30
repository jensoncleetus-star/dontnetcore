var count = 1, type = '';
limits = 500;
function minstockcheck(arg) {
   // var keepstock = $(".minstock_" + arg).attr('data-keeps');
    if (1==1) {
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

        var ItemOutOfStock = $("#ItemOutOfStock").val();

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
            else if (totstock < 0 && ItemOutOfStock == 'inactive') {
                stock = stock.toFixed(2);
                alert("This Item Is Going To Out of Stock!!! Only " + stock + " " + unitname + "Items Are Available In Stock.." + "");
              //  $(".total_qntt_" + arg).val(parseInt(stock));
            }

        } else {
            stock = stock - (qty - quantity);
            var totstock = stock - quantity;
            if (totstock <= minstock && totstock >= 0) {
                alert("Stock Exceeds Minimum Stock");
            }
            if (totstock < 0 && ItemOutOfStock == 'inactive') {
                alert("This Item Is Going To Out of Stock!!! Only " + stock + " " + unitname + " Items Are Available In Stock..");
              //  $(".total_qntt_" + arg).val(stock);
            }

        }
        //if (quantity >= stock && stock <= 0) {
        //    alert("You are Adding The Item In Less Stock..");
        //    $(".total_qntt_" + arg).val(quantity);
        //    stock = stock - (qty - quantity);
        //}
    }
}

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
               "<td class='text-center'><button tabindex='" + tab4 + "' style='text-align: right;' class='btn btn-danger' type='button' value='Delete' onclick='deleteRow(this)'><i class='fa fa-trash fa-1x'></i></button></td>";
        row += data + "</tr>";

        $('#' + t).append(row);
        var date = $("#BOMDate").val();
        var MC = $("#ddlMC").val();
        searchbomItem(date,MC);
        if (itemdata) {
            createUnitList(itemdata, count);
        }
        count++;
        setTabIndex();
    }
}
function quantity_change(arg) {
    minstockcheck(arg);
    if ($('#item_name_'+arg).val() == null) {
        $('#total_qntt_' + arg).val(0);
    }
}
function searchbomItem(date, MC) {
    var selecteditem = new Array();
    $(".item_name").each(function () {
        selecteditem.push($(this).val());
    });

    $(".item_name").select2({
        placeholder: 'Search Item by Code',
        minimumInputLength: 0,
        dropdownAutoWidth: true,
        ajax: {
            url: "/Item/SearchdetailsMCSP2",
            dataType: 'json',
            type: "POST",
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    ItemID: selecteditem,
                    page: params.page || 1,
                    date: date,
                    MC: MC
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
function repoFormatResult(repo) {
    var bg = "";
    if (repo.KeepStock) {
        bg = (parseFloat(repo.total) > 0) ? "" : " text-red;display:none;";
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
        var primary = repo.total;
    
            var p = parseInt(((repo.total / repo.ConFactor) * 100) / 100);
            var sub = (repo.total % repo.ConFactor).toFixed(0);
            total = primary + " " + repo.PriUnit + ", " + primary * repo.ConFactor + " " + repo.SubUnit
        
        if (primary < repo.MinStock) {
            markup += '<div class="se-sec text-yellow">Availability :' + total + '</div>';
        }
        else {
            markup += '<div class="se-sec">Availability :' + total + '</div>';
        }
    }
    

    markup += '</div>';
    var retn = $(markup);
    var total;
    var primary = (repo.total / repo.ConFactor);
    if (repo.total % repo.ConFactor == 0) {
        total = (repo.total / repo.ConFactor);
    }
    else {
        var p = parseInt(((repo.total / repo.ConFactor) * 100) / 100);
        var sub = (repo.total % repo.ConFactor).toFixed(0);
        total = p;
    }
   // if(total>0)
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
   var mc = $("#ddlMC").val();
    $.ajax({
        url: '/Item/GetItemMC',
        type: "GET",
        dataType: "JSON",
        data: {
            itemID: selectObject.value,
            MC: mc,

        },
        success: function (result) {
        
              createUnitList(result, dataid);
                $("#total_qntt_" + dataid).val(1);
                $(selectObject).closest('tr').attr('class', "item_" + result.ItemID);
                if ($(".item_").length == 0) {
                    addbomitem('addbomitem', '','0');
                }
                $('.unit_name_' + dataid).focus();
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
                //if (result.Unit == result.ItemUnitID)
                //    newOption.attr("selected", "selected");
                //if (result.Unit == result.SubUnitId)
                   
            }
            newOption1.attr("selected", "selected");
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
    minstockupdate(result, dataid);
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
        //var r = confirm("Are you sure you want to delete this..?");
        if (1==1) {
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
