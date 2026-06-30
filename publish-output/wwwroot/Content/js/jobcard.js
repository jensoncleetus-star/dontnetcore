var count = 1, type = '';
limits = 500;
//Add Row
function additem(t, action, ItemUnit, ItemTax, ItemTotalAmount, ItemQuantity, Item, ItemCode, ItemName, ItemUnitPrice, ItemSubTotal, ItemWithCode, ItemTaxAmount, PurchasePrice, itemdata) {
    if (count == limits) alert("You have reached the limit of adding " + count + " inputs");
    else {
        var Option = "";
        var optionunit = "";
        var required = "";
        var slno = $('#additem tr').length + 1;
        var a = "item_name" + count,
        tabindex = count * 5;
        var row = "<tr class='item_' id='item_" + count + "'>";
        var data = "";
        var price = 0;
        var baseprice = 0;
        var mrp = 0;
        var htdata = "";


        tab1 = tabindex + 1;
        tab2 = tabindex + 2;
        tab3 = tabindex + 3;


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


        if (itemdata) {

            price = itemdata.SellingPrice;
            baseprice = itemdata.BasePrice;
            mrp = itemdata.MRP;

            htdata = "<div class='minstock_" + count + "'";
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
        var itemaddbtn = "<span class='input-group-btn'><a type='button' href='/Item/AddItem' class='modal-create-lg btn btn-success btn-flat'><i class='fa fa-1x fa-plus-circle'></i></a></span>";


        data = "<td class='text-center'> " + slno + " </td>" +
                "<td class='input-group input-group-sm'><select data-name='Item' class='form-control item_name' " + required + " data-id='" + count + "' placeholder='Item Name' id='item_name_" + count + "'  data-val-required='The Item field is required' onchange='GetItemdetails(this," + count + ",\"" + type + "\")'>" + Option + "</select> " + itemaddbtn + "</td>" +
                "<td><input data-name='ItemTotalAmount' type='text' name='product_rate[]' " + required + " onchange='rate_change(" + count + ",\"" + type + "\");' id='price_item_" + count + "' value='" + ItemTotalAmount + "' class='price_item_" + count + " form-control text-right sell_price' placeholder='0.00' min='0' tabindex='" + tab2 + "'/><input type='hidden' data-value='" + price + "' value='" + baseprice + "' name='base_rate' id='base_rate_" + count + "'> </td> " +
                "<td class='text-center'><button tabindex='" + tab3 + "' style='text-align: right;' class='btn btn-danger' type='button' value='Delete' onclick='deleteRow(this)'><i class='fa fa-trash fa-1x'></i></button>" + htdata + " <input type='hidden' value='" + PurchasePrice + "' class='purprice' name='purprice' id='purprice_" + count + "'/></td>";
        row += data + "</tr>";
        $('#' + t).append(row);
        // $('#item_ .item_name').focus();
        searchItem();
        if (itemdata) {
            rate_change(count, type, 'foredit');
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
        if (ItemId != null) {
            if ($(".item_" + ItemId).length > 1) {
                if ($(".item_" + ItemId).length == 2) {
                    alert("Cannot Add an Item More than 2 times !!");
                    $("#item_name_" + dataid).val(null).trigger("change");
                }
            } else {
                if ($(".item_" + ItemId).length > 0) {
                    if (confirm('Are you sure want to Add this item Again?')) {
                        itemUpdate(selectObject, dataid, action);
                    }
                    else {
                        $("#item_name_" + dataid).val(null).trigger("change");
                    }
                }
                else {
                    itemUpdate(selectObject, dataid, action);
                }
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
            $(".price_item_" + dataid).val(result.SellingPrice);
            CalculatetblItemListSum();

            $(selectObject).closest('tr').attr('class', "item_" + result.ItemID);
            if ($(".item_").length == 0) {
                additem('additem', '', '', '0.00', '0.00', '0');
            }
        }
    });
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
            url: "/Item/SearchItem",
            dataType: 'json',
            type: "POST",
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    ItemID: selecteditem,
                    page: params.page || 0
                };
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
    rate_change(count);
}

function rate_change(arg, type, foredit) {
    if ($('#item_name_'+arg).val() != null) {
        CalculatetblItemListSum();
    }
    else {
        $('#price_item_' + arg).val(0);
    }
}
function CalculatetblItemListSum() {
    var tbody = $("#itemtable tbody");
    if (tbody.children().length > 0) {
        var SPrice = 0;
        $(".sell_price").each(function () {
            var subTot = this.value;
            subTot = subTot || 0.00;
            SPrice = parseFloat(SPrice) + parseFloat(subTot);
        });
        $("#totalAmt").text((SPrice).toFixed(2));
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
    $('#additem tr').each(function () {
        $(this).find('td:first').text(i);
        i++;
    });
    CalculatetblItemListSum();
}

function JcSubmit(fnval, ret) {
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
    var data = HTMLtbl.getData($('#itemtable'));

    var parameters = {};
    parameters.jcitems = data;
    parameters.JobCardNo = $('#JobCardNo').val();
    parameters.JCDate = $('#JCDate').val();
    parameters.Customer = $('#ddlCustomer').val();
    parameters.Mechanic = $('#ddlMechanic').val();
    parameters.ReceivedBy = $('#ddlReceivedBy').val();
    parameters.PWCModel = $('#PWCModel').val();
    parameters.Details = $('#Details').val();
    parameters.TotalAmount = $('#totalAmt').text();
    parameters.Branch = $('#ddlBranch').val();
    parameters.ApprovedBy = $('#SelApprovedBy').val();
    parameters.action = fnval;

    parameters.Ref1 = $('#Ref1').val();
    parameters.Ref2 = $('#Ref2').val();
    parameters.Ref3 = $('#Ref3').val();
    parameters.Ref4 = $('#Ref4').val();
    parameters.Ref5 = $('#Ref5').val();


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
            if (e.status == true) {
                if (fnval == "print") {
                    PrintJobCard(e);
                    //alert(ret);                    
                } else {
                    $('.ajax_response', res_success).text(e.message);
                    $('.AlertDiv').prepend(res_success);
                }
                if (fnval == "update" || ret == "redindex") {
                    setInterval(window.location.href = '/JobCard/Index', 120);
                }                
                else {
                    location.reload();
                }                

            } else {
                $('.ajax_response', res_danger).text(e.message);
                $('.AlertDiv').prepend(res_danger);
                $("button").prop('disabled', false); // enable button
            }
        }
    });
    function PrintJobCard(e,fmapp) {
        $("#lblCardNo").text(e.summary.CardNo);
        $("#lblDate").text(convertToDate(e.summary.Date));
        $("#lblEmployee").text(e.summary.Employee);
        $("#lblMechName").text(e.summary.MechName);
        $("#lblPWCModel").text(e.summary.PWCModel);

        $("#lblDetails").text(e.summary.Details);

        $("#lbltrn").text(e.summary.TRN);
        // bind Party details
        $("#lblParty").text(e.summary.PartyName);

        if (e.summary.ComHeadCheck == 0) {
            $("#ComHeadCheck").hide();
        }
        else {
            $("#ComHeadCheck").show();
        }
        var details = "";
        // var remark = e.summary.Details.replace(/\n/g, "<br/>");
        if (e.summary.Address != null) {
            details += e.summary.Address;
        }
        if (e.summary.City != null) {
            details += "<br />" + e.summary.City;
        }
        else if (e.summary.State != null) {
            details += "<br />" + e.summary.State;
        }
        else if (e.summary.Country != null) {
            details += "<br/>" + e.summary.Country;
        }
        else if (e.summary.Zip != null) {
            details += "<br />" + e.summary.Zip;
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
        if (e.summary.Email) {
            details += "<br/> Email : " + e.summary.Email
        }
        $("[id$=lbladdress]").html(details);

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


        var str2 = "";
        var count = 2;
        var str1 = "";
        var str3 = "";

        // bind items
        var itemsData = bindItem(e);


        $('#itemtable ').append(itemsData);


        var grt = parseFloat(e.summary.TotalAmount).toFixed(2);
        // bind total section
        var word = conNumber(grt);
        //if (e.summary.Discount > 0) {
        //    str2 += "<td>Discount خصم</td><td id='discountprint' class='text-right'>" + parseFloat(e.summary.Discount).toFixed(2) + "</td></tr> ";
        //    count++;
        //    str2 += "<tr class='border-top'><td>VAT<span style='direction:ltr'>(5.00%)</span> برميل</td><td  class='text-right'>" + parseFloat(e.summary.TaxAmount).toFixed(2) + "</td></tr>";
        //}
        //else {
        //    str2 += "<td>VAT<span style='direction:ltr'>(5.00%)</span> برميل </td><td class='text-right'>" + parseFloat(e.summary.TaxAmount).toFixed(2) + "</td></tr>";
        //}
        //if (type != "nobillsundry") {
        //    // bind bill sundry
        //    str2 += bindSundry(e);
        //    if (e.billsundry.length > 0) {
        //        count += e.billsundry.length;
        //    }
        //}
        var wordHtml = "<tr class='border-top'><td colspan='6' rowspan='2' style='padding-right: 20px;'><strong>" + word + " Only</strong></td><td>Amount كمية</td><td class='text-right'>" + parseFloat(e.summary.TotalAmount).toFixed(2) + "</td></tr>";
        str2 += "<tr class='border-top'><th>Total المبلغ الإجمالي(aed)</th><th class='text-right'>" + grt + "</th></tr>";

        //colspan='6' rowspan='" + count + "'
        //  str2 += "<tr class='border-top'><td colspan='8'><strong><u>Details :</u></strong><br/>" + remark + " </td></tr>";

        str1 = wordHtml + str2;
        $('#itemtable1').append(str1);
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
        $('title').html(e.summary.CardNo);
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
        window.print();
    }
    function bindItem(e) {
        var total = parseFloat(0);
        var str = "";
        var count = 1;
        $.each(e.item, function (i, item) {
            var subtot = parseFloat(item.ItemTotalAmount.toFixed(2));
            total += subtot;
            str += '<tr>';
            str += '<td>' + count + '</td>';
            str += '<td>' + item.ItemName + '</td>';
            str += '<td>' + item.ItemTotalAmount + '</td>';
            //str += '<td class="text-right">' + total.toFixed(2) + '</td>';
            str += '</tr>';
            count++;
        });
        return str;
    }
}

function JcItemSubmit(fnval) {
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
    var data = HTMLtbl.getData($('#itemtable'));

    var parameters = {};
    parameters.jcitems = data;
    var url = $('#createForm')[0].action;

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
                window.location.href = '/JobCard/JobCardSetting';
            } else {
                $('.ajax_response', res_danger).text(e.message);
                $('.AlertDiv').prepend(res_danger);
                $("button").prop('disabled', false); // enable button
            }
        }
    });
}