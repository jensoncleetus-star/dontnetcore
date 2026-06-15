var count = 1, type = '';
limits = 500;
//Add Row 
var pcount = 2;
function addrow(t, action, ItemUnit, ItemQuantity, Item, ItemCode, ItemName, ItemWithCode, ItemNote, itemdata, ItemMake, ItemMakeName, ItemRemark, ItemUnitPrice) {

    if (count == limits) alert("You have reached the limit of adding " + count + " inputs");
    else {
        var Option = "";
        var optionunit = "";
        var OptionPro = "";
        var OptionRemark = "";
        var required = "";
        var slno = $('#addinvoiceItem tr').length + 1;
        var a = "item_name" + count,
            tabindex = count * 5;
        var row = "<tr class='item_' id='item_" + count + "'>";
        var data = "";

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
        tab8 = tabindex + 8;
        tab9 = tabindex + 9;

        if (itemdata != null && itemdata.ItemMake != null && itemdata.ItemMakeName != null) {
            OptionPro = "<option value='" + ItemMake + "'>" + ItemMakeName + "</option>"
        }

        if (ItemRemark == null) {
            ItemRemark = "";
        }
        if (Item != null) {
            row = "<tr class='item_" + Item + "' id='item_" + count + "'>";
            Option = "<option value='" + Item + "'>" + ItemWithCode + "</option>";

        }

        if (ItemQuantity == 0)
            ItemUnitPrice = 0;

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
            '<div class="form-group"><textarea name="itemnote" cols="40" rows="10" class="form-control itemnote" id="itemnote-' + count + '" maxlength="1000">' + inote + '</textarea></div>' +
            '<div class="form-group"><button class="btn btn-info" type="button" data-dismiss="modal">Save</button></div>' +
            '</div></div></div>';
        notbtn = "<button type='button' class='itnote btn btn-default btn-flat' data-toggle='modal' data-target='#modal-item-" + count + "'><i class='fa fa-1x fa-file-text-o'></i></button>";

        var itemaddbtn = "<span class='input-group-btn'><a type='button' href='/Item/AddItem' class='modal-create-lg btn btn-success btn-flat'><i class='fa fa-1x fa-plus-circle'></i></a>" + notbtn + "</span>";
        var brandaddbtn = "<span class='input-group-btn'><a type='button' href='/ItemBrand/Create' class='btnbrandAdd btn btn-success btn-flat'><i class='fa fa-1x fa-plus-circle'></i></a></span>";
        var notes = "";
        var makecheck = $("#makecheck").val();

        data = "<td class='text-center' id=" + divid + "> " + slno + " </td>" +
            "<td class='input-group input-group-sm'><select class='form-control item_name' " + required + " data-id='" + count + "' placeholder='Item Name' id='item_name_" + count + "'  data-msg-required='The Item field is required' onchange='GetItemdetails(this," + count + ",\"" + type + "\")'>" + Option + "</select> " + itemaddbtn + "</td>" +
            "<td style='width:100px;'><select class='form-control units unit_name_" + count + "' id='unit_name_" + count + "' data-id='" + count + "' id='unit_name' onchange='unitchange(this," + count + ",\"" + type + "\"); '></select></td>" +
            "<td> <input type='number' name='product_quantity[]' data-msg-min ='The Item Quantity must be Greater than Zero' onchange='quantity_change(" + count + ");' id='total_qntt_" + count + "' value='" + ItemQuantity + "'  class='total_qntt_" + count + " form-control text-right quty' placeholder='0' value='0' min='.01' tabindex='" + tab2 + "'/></td>" +
            "<input type='hidden' class='selitem_name_" + count + "' value='" + ItemName + "' name='selitem_name' id='selitem_name_" + count + "'/> " +
            "<input type='hidden' class='selitem_code_" + count + "' value='" + ItemCode + "' name='selitem_code' id='selitem_code_" + count + "'/> " +
            "<input type='hidden' class='selitem_category_" + count + "' value='" + ItemCategory + "' name='selitem_category' id='selitem_category_" + count + "'/> " +
            "<input type='hidden' class='itemremark" + count + "' value='" + ItemRemark + "' name='itemremark' id='itemremark" + count + "'/> " +
            "</td>" +
            "<td><input type='number' name='product_rate[]'  id='price_item_" + count + "' value='" + ItemUnitPrice + "' class='price_item_" + count + " form-control text-right totrate' placeholder='0.00' value='0' min='0' onchange='price_change(" + count + ");' tabindex='" + tab3 + "'/></td> "
            ;

        notes += "<td class='text-center'><button tabindex='" + tab5 + "' style='text-align: right;' class='btn btn-danger' type='button' value='Delete' onclick='deleteRow(this)'><i class='fa fa-trash fa-1x'></i></button>" + itemnote + "</td>";
        var checkbr = "";
        if (makecheck == "active") {
            checkbr = "<td  class='input-group input-group-sm'><select class='form-control brand_name' name='brand_name' data-id='" + count + "' placeholder='brand' id='brand_name_" + count + "'>" + OptionPro + "</select>" + brandaddbtn + "</td>" +
                "<td><textbox><input type='text' data-name= 'itemremark' id='itemremark' value='" + ItemRemark + "' class=  'itemremark form-control text-right itemremark valid' placeholder='Item Remark'></textbox></td>"
        }
        else {
            checkbr = "<input type='hidden' class='hidebrand_name' value='0' name='brand_name' id='brand_name_" + count + "'/>" +
                "<td><textbox><input type='text' data-name= 'itemremark' id='itemremark' value='" + ItemRemark + "' class=  'itemremark form-control text-right itemremark valid' placeholder='Item Remark'></textbox></td>"
        }
        row += data + checkbr + notes + "</tr>";
        $('#' + t).append(row);

        $('#itemnote-' + count).wysihtml5();

        if (Item != null) {
            $("#total_qntt_" + count).attr('readonly', false);
        }

        searchItem();
        searchbrand();

        if (itemdata) {
            createUnitList(itemdata, count, action);
        }
        CalculatetblItemListSum();
        count++;
        setTabIndex();

    }
}

//item details
function GetItemdetails(selectObject, dataid, action) {
    $("#total_qntt_" + dataid).attr('readonly', false);
    if (selectObject.value) {
        var ItemId = selectObject.value;
        if (ItemId != null) {
            if ($(".item_" + ItemId).length > 0) {
                if ($(".item_" + ItemId).length < 10) {
                    if (confirm('Are you sure want to Add this item Again?')) {
                        itemUpdate(selectObject, dataid, action);
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
            }
        }
    }
}

function itemUpdate(selectObject, dataid, action) {
 
    $.ajax({
        url: '/Item/GetItem',
        type: "GET",
        dataType: "JSON",
        data: { itemID: selectObject.value, },
        success: function (result) {
            createUnitList(result, dataid, action);
            $("#total_qntt_" + dataid).val(1);

            //If Item is Bundle Item - Show SalesPrice(Otherwise PurchasePrice)
            if (result.ItemBundleId == 0 || result.ItemBundleId == null)
                $(".price_item_" + dataid).val(result.PurchasePrice);
            else
                $(".price_item_" + dataid).val(result.SellingPrice);

            CalculatetblItemListSum();


            if ((action != "sales" && action != "preturn") || ((action == "sales" || action == "preturn") && result.KeepStock != true) || ((action == "sales" || action == "preturn") && result.KeepStock == true && result.total > 0)) {
                // append item unit list 

                $(selectObject).closest('tr').attr('class', "item_" + result.ItemID);
                if (action == "sales" || action == "preturn") {

                    minstockupdate(result, dataid);
                }
                if ($(".item_").length == 0) {
                   // addrow('addinvoiceItem', 'purchase', "0.00", "0");
                    addrow('addinvoiceItem', 'purchase', "", 0, "", "", "", "", "", "", "", "", "", 0);
                }
                $('.unit_name_' + dataid).focus();
            }
            else if ((result.KeepStock == true && result.CheckStock == 0 && result.total <= 0)) {

                var res = confirm("Are you Sure Want To Add Items In Less Stock ?");
                if (res == true) {
                    $(selectObject).closest('tr').attr('class', "item_" + result.ItemID);
                    if (action == "sales" || action == "preturn") {
                        minstockupdate(result, dataid);
                    }
                    if ($(".item_").length == 0) {
                        //addrow('addinvoiceItem', 'purchase', "0.00", "0");
                        addrow('addinvoiceItem', 'purchase', "", 0, "", "", "", "", "", "", "", "", "", 0);
                    }
                    $('.unit_name_' + dataid).focus();
                }
                else {
                    $("#item_name_" + dataid).val(null).trigger("change");
                    $("#unit_name_" + dataid).val(null).trigger("change");
                    $("#total_qntt_" + dataid).val(null).trigger("change");

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
                    $("#unit_name_" + dataid).val(null).trigger("change");
                    $("#total_qntt_" + dataid).val(null).trigger("change");

                }
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

//search Brand
function searchbrand() {
    $(".brand_name").select2({
        placeholder: 'Search Item Brand',
        minimumInputLength: 0,
        ajax: {
            url: "/ItemBrand/SearchBrand",
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

    CalculatetblItemListSum();
}

function price_change(arg) {

    CalculatetblItemListSum();
}
//item unit change
function unitchange(selectObject, arg, action) {

    var index = $('#unit_name_' + arg).prop('selectedIndex');
    var unitId = parseFloat($('#unit_name_' + arg).val());
    CalculatetblItemListSum();

}

//Total Quantity
function CalculatetblItemListSum() {

    var qty = $(".ItemQty").val();

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

    var TotalPrice = $(".TotalPrice").val();

    if (TotalPrice != 0) {
        var tbody = $("#normalinvoice tbody");
        if (tbody.children().length > 0) {

            var gtprice = 0;
            var subprice = 0;

            $(".totrate").each(function () {
                subprice = this.value;
                //alert(subprice)
                //if (subprice != 0 && subprice != "undefined" && subprice != "")
                gtprice = parseFloat(gtprice) + parseFloat(subprice);
            });


            $("[id$=ItemCount]").val(tbody.children().length);
            $("[id$=TotalPrice]").text((gtprice).toFixed(2));

        }
    }
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
    var i = 1;
    $('#addinvoiceItem tr').each(function () {
        $(this).find('td:first').text(i);
        i++;
    });

}

//itembind
function PrintInvoiceMRequisition(e, type) {
    $('#Branch').hide();
    $('#project').hide();
    $('#IblApproved').hide();
    $("#lblCName").text(e.cdetails.CName);

    var BillNo = (e.summary.BillNo != null && e.summary.BillNo != "") ? e.summary.BillNo : "";
    $("#lblVoucherNo").text(BillNo);
    $("#lblDate").text(convertToDate(e.summary.Date));

    if (e.summary.ComHeadCheck == 0) {
        $("#ComHeadCheck").hide();
    }
    else {
        $("#ComHeadCheck").show();
    }
    var str = "";
    $.each(e.approval, function (i, approval) {
        $('#IblApproved').show();
        str = '<td colspan="4"><h5><strong>' + approval.approvalBy + '<strong></h5></td>';
    });
    $('#posfootApp').append(str);
    if (e.summary.BranchCheck == 0 && (e.summary.BranchNameCode != null && e.summary.BranchNameCode != "")) {
        $('#Branch').show();
        $("#lblBranch").text(e.summary.BranchNameCode);
    }
    if (e.summary.ProCheck == 0 && (e.summary.PrjNameCode != null && e.summary.PrjNameCode != "")) {
        $('#project').show();
        $("#lblProject").text(": " + e.summary.PrjNameCode);
        var textvalue = $("#ddlProTask option:selected").text();
        $("#lblTask").text(": " + textvalue);//e.summary.TaskName
    }

    $("#lblRemark").text(e.summary.Remarks);
    var itemsData = bindPOSItem(e);
    $('#itemtable').append(itemsData);
    $("#IblRValidity").text(convertToDate(e.summary.validity));
    $("#lblRequestedBy").text(e.summary.Cashier);


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
    $('body').html(printContent);
    var titlename = "_invoice - " + e.summary.CreatedBy + " - " + e.summary.BillNo;
    $('title').html(titlename);
    window.print();
}

function bindPOSItem(e) {

    var TQunty = parseFloat(0);
    var count = 1;
    var Quantity = parseFloat(0);
    var str = "";
    var PartNoSts = 1;
    var PartNo = ""
    var make = ""
    $('#PartNo').hide();
    $('#make').hide();
    var makecheck = $("#makecheck").val();

    $.each(e.item, function (i, item) {

        var Quantity = parseFloat(item.ItemQuantity.toFixed(2));
        TQunty += Quantity;
        var unit = (item.ItemUnit != null) ? item.ItemUnit : "";
        PartNo = (item.PartNumber != null && item.PartNumber != "") ? item.PartNumber : "";
        var itemnote = "";
        if (item.ItemNote != "" && (typeof item.ItemNote != 'undefined')) {
            itemnote = "<br /><small>" + item.ItemNote + "</small>";
        }

        if (item.bundle != null && item.bundle.length > 0) {
            var desc = "<br/>[<span class='descr' data-name='itemNote'>";
            $.each(item.bundle, function (j, itemss) {
                desc += itemss.ItemCode + " - " + itemss.ItemName;
                desc += " - " + parseFloat(itemss.ItemQuantity).toFixed(2) + " ";
                desc += (itemss.ItemUnit != null) ? itemss.ItemUnit : "";
                desc += "<br/>";
            });
            desc += "</span>]";

        }
        if (desc != null && desc != 'undefined')
            itemnote = itemnote + desc;
        else
            itemnote = itemnote;
        make = (item.Make != null && item.Make != "") ? item.Make : "";
        str += '<tr>';
        str += '<td>' + count + '</td>'
        if (item.PNoStatus == 0) {
            PartNoSts = 0
            $('#PartNo').show();
            str += '<td>' + PartNo + '</td>'
        }

        str += '<td>' + item.ItemCode + "-" + item.ItemName + "  " + itemnote + '</td>';
        if (makecheck == "active") {
            $('#make').show();
            str += '<td class="text-center">' + make + '</td>';
        }
        str += '<td>' + unit + '</td>';
        str += '<td class="text-right">' + parseFloat(item.ItemQuantity).toFixed(2) + '</td>';
        str += '<td class="text-right">' + item.ItemRemark + '</td>';
        str += '</tr>';
        count++;
    });

    var Row = "";
    Row += '<tr class="border-top">';
    Row += '<td><h5><strong>' + "Total" + '</h5></strong></td>';
    if (PartNoSts == 0) {
        var PartNo = (PartNo != null && PartNo != "") ? PartNo : "";
        Row += '<td><h5><strong>' + PartNo + '</h5></strong></td>';
    }
    Row += '<td>' + "" + '</td>';
    Row += '<td>' + "" + '</td>';
    if (makecheck == "active") {
        Row += '<td>' + "" + '</td>';
    }
    Row += '<td class="text-right"><h5><strong>' + parseFloat(TQunty).toFixed(2) + '</h5></strong></td>';
    Row += '<td>' + "" + '</td>';
    Row += '</tr>';
    $('#posfootR').append(Row);
    return str;
}

function fieldReset() {
    $("#ItemQty").text(0.00);
    $("#TotalPrice").text(0.00);
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



