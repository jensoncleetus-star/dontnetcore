var count = 1, type = '';
limits = 500;
function addbomitem(t, action, Quantity, Unit, Item, ItemWithCode, ItemNote, itemdata, note, ItemUnitName) {

    if (count == limits) alert("You have reached the limit of adding " + count + " inputs");
    else {
        var data = "";
        var Type = "";
        var Option = "";
        var OptionUnit=""
        var itemnote = "";
        var notbtn = "";
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
        if (Item != null) {

            row = "<tr class='item_" + Unit + "' id='item_" + count + "'>";
            OptionUnit = "<option value='" + Unit + "'>" + ItemUnitName + "</option>";
        }
        if (count == 1) {
            required = 'required="required"';
        }
        if (action != '') {
            type = action;
        }
        var inote = "";
        if (note) {
            inote = note;
        }
        if (ItemNote) {
            inote = ItemNote;
        }
        itemnote = '<div id="modal-item-' + count + '" class="modal fade" role="dialog" aria-hidden="true"><div class="modal-dialog"><div class="modal-content">' +
            '<div class="form-group"><textarea name="itemnote" cols="40" rows="10" class="form-control itemnote" id="itemnote-' + count + '" maxlength="1000">' + inote + '</textarea></div>' +
            '<div class="form-group"><button class="btn btn-info" type="button" data-dismiss="modal">Save</button></div>' +
            '</div></div></div>';
        notbtn = "<button type='button' class='itnote btn btn-default btn-flat' data-toggle='modal' data-target='#modal-item-" + count + "'><i class='fa fa-1x fa-file-text-o'></i></button>";
      
        
        var itemaddbtn = "<span class='input-group-btn'><a type='button'  class='modal-create-lg btn btn-success btn-flat'><i class='fa fa-1x fa-plus-circle'></i></a>" + notbtn + "</span>";

        data = "<td class='text-center'> " + slno + " </td>" +
            "<td class='input-group input-group-sm'><select  class='form-control item_name' " + required + " data-id='" + count + "' placeholder='Item Name' id='item_name_" + count + "'  data-val-required='The Item field is required' onchange='GetItemdetails(this," + count + ",\"" + type + "\")'>" + Option + "</select> " + itemaddbtn + "</td>" +
            "<td style='width:100px;'><select  class='form-control units unit_name_" + count + "' id='unit_name_" + count + "' " + required + "data-id='" + count + "' id='unit_name' tabindex='" + tab2 + "'> " + OptionUnit + "</select></td>" +
            "<td style='width:100px;'> <input type='number' name='product_quantity[]' onchange='quantity_change(" + count + ");' id='total_qntt_" + count + "' value='" + Quantity + "'  class='total_qntt_" + count + " form-control text-right quty' placeholder='0' value='0' min='.01' tabindex='" + tab3 + "'/></td>" +
            "<td class='text-center'><button tabindex='" + tab4 + "' style='text-align: right;' class='btn btn-danger' type='button' value='Delete' onclick='deleteRow(this)'><i class='fa fa-trash fa-1x'></i></button>" + itemnote + "</td>";

       
        row += data + "</tr>";

        $('#' + t).append(row);
        $('#itemnote-' + count).wysihtml5();

        var date = $("#BOMDate").val();
        var MC = $("#ddlMC").val();
        searchbomItem(date);
        if (itemdata) {
            createUnitList(itemdata, count);
        }
        count++;
        setTabIndex();
    }
}

function quantity_change(arg) {
    if ($('#item_name_' + arg).val() == null) {
        $('#total_qntt_' + arg).val(0);
    }
}
function searchbomItem(date) {
    var selecteditem = new Array();
    $(".item_name").each(function () {
        selecteditem.push($(this).val());
    });

    $(".item_name").select2({
        placeholder: 'Search Item by Code',
        minimumInputLength: 0,
        ajax: {
            url: "/Item/DateWiseStockInItem",
            dataType: 'json',
            type: "POST",
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    ItemID: selecteditem,
                    page: params.page || 1,
                    date: date,

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
            $(selectObject).closest('tr').attr('class', "item_" + result.ItemID);
            if ($(".item_").length == 0) {
                addbomitem('addbomitem', '', '0');
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
                var cols = [];
                $(this).find('input,textarea,select').each(function (colIndex, c) {
                    cols.push($(this).val());
                });
                data.push(cols);
            });
            return data;
        }
    }

   
    var bomdata = {

        'QuotNo': $('#QuotNo').val(),
        'BoqId': $('#BoqId').val(),
        'BoqNo': $('#BoqNo').val(),
        'BillNo': $('#BillNo').val(),
        'Customer': $('#ddlCustomer').val(),
        'SalesExecutive': $('#ddlEmployee').val(),
        

    }
   
    var getQueryString = function (field, url) {
        var href = url ? url : window.location.href;
        var id = href.substring(href.lastIndexOf('/') + 1);
        return id ? id : null;
    }

    var BoqID = getQueryString('');
    var data = HTMLtbl.getData($('#bomtable'));
    var LayoutData = $('#ddlPrintlayout').val();
    var QuoteId = $('#ConTypeId').val();
    var QuoteType = $('#ConType').val();
    
    var parameters = {};
    parameters.bomdata = bomdata;
    parameters.action = fnval;
    parameters.Layout = LayoutData;
    parameters.boqitemz = data;

    parameters.id = BoqID;
    parameters.conid = QuoteId;
    parameters.contype = QuoteType;
  
    parameters.BOQDate = $('#BoqDate').val();

    var url = "";
    if (fnval == "save")
    {
       

        submitForm(fnval, parameters)

        
    }
    if (fnval == "update") {
        url = $('#updateForm')[0].action;

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
                    window.location.href = '/Boq/Index';
                } else {
                    location.reload();
                }
                $("button").prop('disabled', false); // enable button
            }
        });
    }

    
    if (fnval == "print")
    {
      
            
            submitForm(fnval, parameters);
    }


   



}