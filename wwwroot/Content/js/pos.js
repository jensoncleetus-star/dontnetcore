// filter order list

//item pop up
function AddItemPopUp() {
    /* function for Create popup for item large */
    $('table').on('click', '.modal-create-lg', function (e) {
        e.preventDefault();
        $(this).attr('data-target', '#modal-create-lg');
        $(this).attr('data-toggle', 'modal');

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
                        // $('.ajax_response', res_success).text(e.message);
                        //  $('.AlertDiv').prepend(res_success);
                        //   window.location.href = '@Url.Action("Create", "Item")';
                    }
                    $('#modal-create-lg').modal('hide');
                    $('#modal-create-lg').removeData('bs.modal');

                    //$('#modal-container-barcode').modal('hide');
                    //$('#modal-container-barcode').removeData('bs.modal');
                }
                else {
                    //  $('.ajax_response', res_danger).text(e.message);
                    // $('.AlertDiv').prepend(res_danger);
                }
                fadeAlert();
            }
        });


        function PrintBarcode(e) {
            var count = e.item.PCount;
            var itemName = e.item.ItemName;
            var barCode = e.item.Barcode;

            printSticker('barcode', barCode);
            var image = $("#cont").html();
            var CName = $("#cname").val();

            var i = 0;
            var table = "";
            var str = '<h4>' + CName + '</h4>' + image + '<br><strong>' + itemName + '</strong>';
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

            var divToPrint = $("#printitbr").html();
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
                    //  $('.ajax_response', res_danger).text(data.message);
                    //   $('.AlertDiv').prepend(res_danger);
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
                    //   $('.ajax_response', res_danger).text(data.message);
                    //   $('.AlertDiv').prepend(res_danger);
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
                    //   $('.ajax_response', res_danger).text(data.message);
                    //   $('.AlertDiv').prepend(res_danger);
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
                    //  $('.ajax_response', res_danger).text(data.message);
                    //  $('.AlertDiv').prepend(res_danger);
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
                    //  $('.ajax_response', res_danger).text(data.message);
                    //  $('.AlertDiv').prepend(res_danger);
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
                    // $('.ajax_response', res_danger).text(data.message);
                    //  $('.AlertDiv').prepend(res_danger);
                }
                fadeAlert();
            }
        })

        e.preventDefault();
    })

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
                    // $('.ajax_response', res_danger).text(data.message);
                    //  $('.AlertDiv').prepend(res_danger);
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
                    // $('.ajax_response', res_danger).text(data.message);
                    //  $('.AlertDiv').prepend(res_danger);
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

}
function items(divid) {
    $.ajax({
        async: true,
        cache: false,
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/Item/AllItem",
        data: JSON.stringify({ type: 'POS' }),
        success: function (item) {
            $('#' + divid).append(itemlist(item));
            try {
                filterData.init('#portfoliolist', '.portfolio', '.filter');
            }
            catch (x) {
                alert("Error");
            }
            slimScrolls.init('#portfoliolist');
            slimScrolls.init('#itscrol');
        }
    });
}
var slimScrolls = {
    init: function (e) {
        // MixItUp plugin
        // http://mixitup.io
        $(e).slimScroll({
            alwaysVisible: true
        });
    },
    del: function (e) {
        $(e).slimScroll('destroy');
    }
};
//item details
function itemDetails2(dataid, i, action) {
    
    if (dataid != null) {
        $.ajax({
            url: '/ItemBundle/GetItem',
            type: "POST",
            dataType: "JSON",
            data: { itemID: dataid },
            success: function (result) {
                displayunitcheck(result, i, action);
                $(".price").prop("readonly", true);
            }
        });
    }
}
function itemDetails(dataid, i, action) {

    if (dataid != null) {
        $.ajax({
            url: '/ItemBundle/GetItem',
            type: "POST",
            dataType: "JSON",
            data: { itemID: dataid },
            success: function (result) {
                displayunitcheck(result, i, action);
                //reinit();
            }
        });
    }
}
function displayunitcheck(result, i, action) {
    if (result.item.ItemUnitID != null && action == null) {
        if ((result.item.PriUnit != result.item.SubUnit) && result.item.SubUnitId != null) {
            //model pop up
            itemunitpopup(result, i, "sales");
        } else {
            i = parseFloat(i) || 1;
            createRow(result, i, "sales", result.item.ItemUnitID);
        }
    } else {
        i = parseFloat(i) || 1;
        createRow(result, i, "sales", result.item.ItemUnitID, action);
    }
}

//pop up for unit selection when item select
function itemunitpopup(datas, i, action) {
    var data;
    if (typeof datas.item === 'undefined') {
        data = datas;
    }
    else {
        data = datas.item
    }
    $("#selectitem").val("");
    $('#ItemUnitSelect').empty();
    if (data.ItemUnitID != null) {
        var newOption = $('<option></option>');
        if ((data.PriUnit != data.SubUnit) && data.SubUnitId != null) {
            newOption.val(data.ItemUnitID).html(data.PriUnit);

            var newOption1 = $('<option></option>');
            newOption1.val(data.SubUnitId).html(data.SubUnit);
            if (data.ItemUnit) {
                if (data.ItemUnit == data.ItemUnitID)
                    newOption.attr("selected", "selected");
                if (data.ItemUnit == data.SubUnitId)
                    newOption1.attr("selected", "selected");
            }

            $('#ItemUnitSelect').append(newOption);
            $('#ItemUnitSelect').append(newOption1);
        }
        else {
            newOption.val(data.ItemUnitID).html(data.PriUnit);
            $('#ItemUnitSelect').append(newOption);
        }
    }

    $("#selectitem").val(data.ItemID);
    $("#selitemname").text(data.ItemName);

    $('#modal-itemunit').modal("show");
}



//// add item to bill entries
//function createRow(datas, i, action) {
//    var data;
//    if (typeof datas.item === 'undefined') {
//        data = datas;
//    }
//    else {
//        data = datas.item
//    }

//    if (action != "sales" || (action == "sales" && data.KeepStock != true) || (action == "sales" && data.KeepStock == true && data.total > 0)) {
//        var count = $('.price_main').length;
//        var divid = "rowitem_" + data.ItemID;
//        var qunty = "";
//        if (data.ItemQuantity != null) {
//            qunty = data.ItemQuantity;
//        } else {
//            i = parseFloat(i) || 1;
//            qunty = i;
//        }
//        var quantity = 1;
//        //var htdata = "";
//        var rate = parseFloat(data.price);
//        var subtotal = parseFloat(quantity) * parseFloat(rate);
//        var tax = parseFloat(data.Tax);
//        var taxAmount = parseFloat(subtotal) * (parseFloat(tax) / 100);

//        var totalamount = subtotal + taxAmount;

//        var btnadd = "<span id='" + data.ItemID + "' class='input-group-btn qtyadd qtybtn'><i class='fa fa-1x fa-plus-circle'></i></span>";
//        var btnsub = "<span id='" + data.ItemID + "'class='input-group-btn qtysub qtybtn'><i class='fa fa-1x fa-minus-circle'></i></span>";


//        //----for min stock------------
//        minstockupdate(data, data.ItemID);


//        var htdata = "<div class='minstock_" + data.ItemID + "'";
//        if (data.KeepStock == true) {
//            var qntmin = 0;
//            if (data.ItemUnit == data.ItemUnitID) {
//                qntmin = qunty * data.ConFactor;
//            }
//            if (data.ItemUnit == data.SubUnitId) {
//                qntmin = qunty;
//            }
//            totalstock = data.total + qntmin;
//            minstock = data.MinStock * data.ConFactor;
//            htdata += " data-keeps='yes' data-min='" + minstock + "' data-confactor='" + data.ConFactor + "' data-stock='" + totalstock + "'>";
//        }
//        else {
//            htdata += " data-keeps='no' >";
//        }
//        if ($(".minstock_" + data.ItemID).length) {
//            $(".minstock_" + data.ItemID).remove();
//        }
//        //-----------------------------


//        var inote = "";
//        if (data) {
//            inote = data.ItemNote;
//        }
//        var itemnote = '<div id="modal-item-' + count + '" class="modal fade" role="dialog" aria-hidden="true"><div class="modal-dialog"><div class="modal-content">' +
//                       '<div class="form-group"><textarea data-name="itemNote" name="itemnote" cols="40" rows="10" class="form-control inote" id="itemnote-' + count + '" maxlength="255">' + inote + '</textarea></div>' +
//                       '<div class="form-group text-center"><button class="btn btn-info" type="button" data-dismiss="modal">Save</button></div>' +
//                       '</div></div></div>';
//        var notbtn = "<a class='itnote' data-toggle='modal' data-target='#modal-item-" + count + "'> " + data.ItemCode + ' - ' + data.ItemName + "</a>";


//        if ($("#" + divid).length == 0) {
//            var desc = "<span class='descr' data-name='Note'></span>";
//            var exrow = "";
//            if (data.type == "Bundle") {
//                desc = "<br/>[<span class='descr' data-name='Note'>";
//                $.each(datas.bundle, function (i, item) {
//                    desc += item.ItemCode + " - " + item.ItemName;
//                    desc += " - " + (item.quantity).toFixed(2) + " ";
//                    desc += (item.ItemUnitName != null) ? item.ItemUnitName : "";
//                    desc += "<br/>";
//                });
//                desc += "</span>]";
//            }
//            else if (data.Note != "" && data.Note != null && (typeof data.Note !== "undefined")) {
//                desc = "<br/>[<span class='descr' data-name='Note'>" + data.Note + "</span>]";
//            }


//            //------------item unit---------------
//            var selectunit = "<select class='item_unit' data-name='ItemUnit' ><option value=''></option></select>";
//            if (data.ItemUnitID != null) {
//                if ((data.PriUnit != data.SubUnit) && data.SubUnitId != null) {
//                    var sel1 = "";
//                    var sel2 = "";
//                    if (data.ItemUnit) {
//                        if (data.ItemUnit == data.ItemUnitID)
//                            sel1 = "selected";
//                        if (data.ItemUnit == data.SubUnitId)
//                            sel2 = "selected";
//                    }//onchange='unitchange(this," + data.ItemID + ");
//                    selectunit = "<select class='item_unit' id='item_unit_" + data.ItemID + "' data-name='ItemUnit' onchange='unitchange(this," + data.ItemID + ")';><option " + sel1 + " value=" + data.ItemUnitID + ">" + data.PriUnit + "</option><option " + sel2 + " value=" + data.SubUnitId + ">" + data.SubUnit + "</option></select>";
//                }
//                else {
//                    selectunit = "<select class='item_unit' id='item_unit_" + data.ItemID + "' data-name='ItemUnit' ><option value=" + data.ItemUnitID + ">" + data.PriUnit + "</option></select>";
//                }
//            }
//            //--------------------------

//            count++;
//            rowdata = '<div class="price_main" id="' + divid + '">' +
//                            '<input type="hidden" data-name="Item" value="' + data.ItemID + '">' +
//                            '<div class="col-sm-1">' + count + '</div>' +
//                            '<div class="col-sm-3">' + notbtn + desc + '</div>' +
//                            '<div class="col-sm-1">' + selectunit + '</div>' +
//                            '<div class="col-sm-2"><div data-name="ItemUnitPrice" class="price">' + data.price.toFixed(2) + '</div></div>' +
//                            '<div class="col-sm-2"><div class="input-group input-group-sm">' + btnsub + '<input name="price_q" id="qty_' + data.ItemID + '" data-name="ItemQuantity" type="text" class="price_q" value=' + qunty.toFixed(2) + ' onchange="qtychange(this,' + data.ItemID + ')";>' + btnadd + '</div><input data-name="ItemTax" type="hidden" class="taxper" value="' + data.Tax.toFixed(2) + '"><input data-name="ItemTaxAmount" type="hidden" class="taxamt" value="' + taxAmount.toFixed(10) + '"><input data-name="BasePrice" type="hidden" id="base_rate_' + data.ItemID + '" value="' + data.BasePrice + '"><input data-name="ConFactor" type="hidden" id="cfactor_' + data.ItemID + '" value="' + data.ConFactor + '"></div>' +
//                            '<div class="col-sm-2"><div data-name="ItemSubTotal" class="subtot">' + data.price.toFixed(2) + '</div></div>' +
//                            '<div class="col-sm-1"><i class="fa fa-trash-o rowtrash" title="Remove"></i></div>' +
//                            '<div class="itdisc hidden" data-name="ItemDiscount">0.00</div>' + itemnote + htdata //added for min stock
//            '</div>';
//            $('#POSForm').append(rowdata);
//        }
//        else {
//            qunty = parseFloat(qunty) || 1;
//            qty = parseFloat($('#' + divid).find("input.price_q").val());
//            qty += qunty;
//            $('#' + divid).find("input.price_q").val(qty.toFixed(2));
//            rowSubTotal(divid);
//        }
//        $('#' + divid).find("input.price_q").focus();
//        itemTotal();
//        var disc_p = parseFloat($("#disc_p").val()) || 0;
//        if (disc_p != 0) {
//            discAmount();
//        }
//        else {
//            var disc_m = parseFloat($("#disc_m").val()) || 0;
//            if (disc_m != 0) {
//                discPerc();
//            }
//        }
//        rowSubTotal('rowitem_' + data.ItemID);
//        // check touck keyboard enable/disable
//        displaynumpad();
//    } else {
//        alert("This Item is Out of Stock!!!");
//    }
//}
function createRow2(datas, i, action, itemunit, stype) {

    datas.KeepStock = false;
    //        result['item'] = allitems[i];
    //        result['item'].KeepStock = false;
    datas["total"] = 1000;
    datas["ItemQuantity"] = 1;
    datas["price"] = allitems[i].SellingPrice;
    var data = datas;
    if (1) {
        var count = $('.price_main').length;
        var divid = "rowitem_" + data.ItemID;
        var qunty = "";
        var quantity;
        if (data.ItemQuantity != null) {
            qunty = data.ItemQuantity;
        } else {
            i = parseFloat(i) || 1;
            qunty = i;
        }
        if (typeof datas.ItemQuantity == 'undefined')
            quantity = 1;
        else
            quantity = datas.ItemQuantity;
        //var htdata = "";
        var rate = parseFloat(data.price);
        var subtotal = parseFloat(quantity) * parseFloat(rate);
        //subtotal=   parseFloat(parseFloat(subtotal) / parseFloat(1 + parseFloat(5 / 100))).toFixed(2);
        var tax = parseFloat(data.Tax);
        var taxAmount =parseFloat(parseFloat(subtotal) / parseFloat(1 + parseFloat(5 / 100))).toFixed(2);// parseFloat(subtotal) * (parseFloat(tax) / 100);
 
        var totalamount = subtotal + taxAmount;

        var btnadd = "<span id='" + data.ItemID + '-' + itemunit + jcount + "' class='input-group-btn qtyadd qtybtn'><i class='fa fa-1x fa-plus-circle'></i></span>";
        var btnsub = "<span id='" + data.ItemID + '-' + itemunit + jcount + "'class='input-group-btn qtysub qtybtn'><i class='fa fa-1x fa-minus-circle'></i></span>";


        //----for min stock------------
        minstockupdate(data, data.ItemID, itemunit);


        var htdata = "<div class='minstock_" + data.ItemID + '-' + itemunit + "'";
        if (data.KeepStock == true) {
            var qntmin = 0;
            if (data.ItemUnit == data.ItemUnitID) {
                qntmin = qunty * data.ConFactor;
            }
            if (data.ItemUnit == data.SubUnitId) {
                qntmin = qunty;
            }
            totalstock = data.total + qntmin;
            minstock = data.MinStock * data.ConFactor;
            htdata += " data-keeps='yes' data-min='" + minstock + "' data-confactor='" + data.ConFactor + "' data-stock='" + totalstock + "'>";
        }
        else {
            htdata += " data-keeps='no' >";
        }
        if ($(".minstock_" + data.ItemID + '-' + itemunit).length) {
            $(".minstock_" + data.ItemID + '-' + itemunit).remove();
        }
        //-----------------------------


        var inote = "";
        if (data) {
            inote = data.ItemNote;
        }
        var itemnote = '<div id="modal-item-' + count + '" class="modal fade" role="dialog" aria-hidden="true"><div class="modal-dialog"><div class="modal-content">' +
            '<div class="form-group"><textarea data-name="itemNote" name="itemnote" cols="40" rows="10" class="form-control inote" id="itemnote-' + count + '" maxlength="255">' + inote + '</textarea></div>' +
            '<div class="form-group text-center"><button class="btn btn-info" type="button" data-dismiss="modal">Save</button></div>' +
            '</div></div></div>';
        var notbtn = "<a class='itnote' data-name='ItName' data-toggle='modal' data-target='#modal-item-" + count + "'> " + data.ItemCode + ' - ' + data.ItemName + "</a>";
        var itemValue = "<div class='item-name hidden' data-name='item-name'>" + data.ItemName + "</div>";

        var crowid = divid + '-' + itemunit;

        //-------------------------------------------------------------------------------------

        if ($("#" + divid + '-' + itemunit).length == 0) {//$("#" + divid).length == 0 && 
            var desc = "<span class='descr' data-name='Note'></span>";
            var exrow = "";
            if (data.type == "Bundle") {
                desc = "<br/>[<span class='descr' data-name='Note'>";
                $.each(datas.bundle, function (i, item) {
                    desc += item.ItemCode + " - " + item.ItemName;
                    desc += " - " + (item.quantity).toFixed(2) + " ";
                    desc += (item.ItemUnitName != null) ? item.ItemUnitName : "";
                    desc += "<br/>";
                });
                desc += "</span>]";
            }
            else if (data.Note != "" && data.Note != null && (typeof data.Note !== "undefined")) {
                desc = "<br/>[<span class='descr' data-name='Note'>" + data.Note + "</span>]";
            }

            //-----------item unit--------------------
            var selectunit = "";
            if (itemunit != null) {
                if (itemunit == data.ItemUnitID) {
                    // selectunit = "<select class='item_unit' id='item_unit_" + data.ItemID + "' data-name='ItemUnit' ><option value=" + data.ItemUnitID + ">" + data.PriUnit + "</option></select>";
                    selectunit = "<label>" + data.PriUnit + "</label><input type='hidden' class='item_unit' id='item_unit_" + data.ItemID + '-' + itemunit + "' data-name='ItemUnit' value=" + data.ItemUnitID + " />";
                }
                if (itemunit == data.SubUnitId) {
                    // selectunit = "<select class='item_unit' id='item_unit_" + data.ItemID + "' data-name='ItemUnit' ><option value=" + data.SubUnitId + ">" + data.SubUnit + "</option></select>";
                    selectunit = "<label>" + data.SubUnit + "</label><input type='hidden' class='item_unit' id='item_unit_" + data.ItemID + '-' + itemunit + "' data-name='ItemUnit' value=" + data.SubUnitId + " />";
                }
            }

            //------------------------------------------

            count++;
            var gtot = (data.price * qunty) + taxAmount;
            //var gtot = data.price + taxAmount;
            crowid = divid + '-' + itemunit;
            rowdata = '<div class="price_main" id="' + divid + '-' + itemunit + '">' +
                '<input type="hidden" class="itemselect" data-name="Item" value="' + data.ItemID + '">' +
                '<div class="col-sm-1">' + count + '</div>' + itemValue +
                '<div class="col-sm-3">' + notbtn + desc + '</div>' +
                '<div class="col-sm-1">' + selectunit + '</div>' +
                //'<div class="col-sm-2"><div data-name="ItemUnitPrice" class="price">' + data.price.toFixed(2) + '</div></div>' +
                '<div class="col-sm-1"><input type="text" style="width: 100%;height: 32px;" data-name="ItemUnitPrice" class="price numbers" value=' + data.price.toFixed(2) + ' id="' + divid + '-' + itemunit + '"></div>' +
                '<div class="col-sm-1"><div class="input-group input-group-sm">' + btnsub + '<input name="price_q" id="qty_' + data.ItemID + '-' + itemunit + '" data-name="ItemQuantity" type="text" class="price_q" value=' + qunty + ' onchange="qtychange(this,' + data.ItemID + ')";>' + btnadd + '</div></div>' +
                '<input data-name="ItemTax" type="hidden" class="taxper" value="' + data.Tax.toFixed(2) + '"><input data-name="BasePrice" type="hidden" id="base_rate_' + data.ItemID + '" value="' + data.BasePrice + '"><input data-name="ConFactor" type="hidden" id="cfactor_' + data.ItemID + '" value="' + data.ConFactor + '">' +
                '<div class="col-sm-1"><input readonly data-name="ItemTaxAmount" type="text" class="taxamt" value="' + taxAmount.toFixed(2) + '" style="width:100%;border:0px;text-align: center;"></div>' +
                '<div class="col-sm-2"><div data-name="ItemSubTotal" class="subtot hidden">' + gtot.toFixed(2) + '</div><div data-name="Itemgtotal" class="gtotal">' + gtot.toFixed(2) + '</div> </div>' +
                '<div class="col-sm-1"><i class="fa fa-trash-o rowtrash" title="Remove"></i></div>' +
                '<div class="itdisc hidden" data-name="ItemDiscount">0.00</div>' + itemnote + htdata //added for min stock
            '</div>';
            $('#POSForm').append(rowdata);
        }
        else {
            qunty = parseFloat(qunty) || 1;
            qty = parseFloat($('#' + divid + '-' + itemunit).find("input.price_q").val());
            qty += qunty;
            $('#' + divid + '-' + itemunit).find("input.price_q").val(qty.toFixed(2));
            rowSubTotal('rowitem_' + data.ItemID + '-' + itemunit);
        }
        if (stype == null) {
            $('#' + divid + '-' + itemunit).find("input.price_q").focus();
        }
        else if (stype == "scan") {
            $("#barcode").focus();
        }
        itemTotal(crowid);
        var disc_p = parseFloat($("#disc_p").val()) || 0;
        if (disc_p != 0) {
            discAmount();
        }
        else {
            var disc_m = parseFloat($("#disc_m").val()) || 0;
            if (disc_m != 0) {
                discPerc();
            }
        }
        //rowSubTotal('rowitem_' + data.ItemID + '-' + itemunit);
        // check touck keyboard enable/disable
        displaynumpad();
    } else {
        alert("This Item is Out of Stock!!!");
    }
}
// create row
var jcount = 100;
function createRow(datas, i, action, itemunit, stype) {
    itemunit = 5;
    if ($('#ddlCustomer').val()==null)
    $('#ddlCustomer').val(7).trigger('change');
    var data;
   // jcount++;
    if (typeof datas.item === 'undefined') {
        data = datas;
    }
    else {
        data = datas.item
    }
    if (action != "sales" || (action == "sales" && data.KeepStock != true) || (action == "sales" && data.KeepStock == true )) {
        var count = $('.price_main').length;
        var divid = "rowitem_" + data.ItemID;
        var qunty = "";
        var quantity;
        if (data.ItemQuantity != null) {
            qunty = data.ItemQuantity;
        } else {
            i = parseFloat(i) || 1;
            qunty = i;
        }
        if (typeof datas.ItemQuantity == 'undefined')
            quantity = 1;
        else
            quantity = datas.ItemQuantity;
        //var htdata = "";
        if (stype == "editp") {
            data.price = data.price; //parseFloat(data.price / parseFloat(1 + parseFloat(data.Tax / 100)));

        }
        else {
            data.price = data.price ;//parseFloat(data.price / parseFloat(1 + parseFloat(data.Tax / 100)));

        }
        var rate = parseFloat(data.price);
        var subtotal = parseFloat(quantity) * parseFloat(rate);
        var tax = parseFloat(data.Tax);
        var taxAmount = parseFloat(subtotal) * (parseFloat(tax) / 100);
taxAmount=parseFloat(subtotal / parseFloat(1 + parseFloat(data.Tax / 100)));
subtotal=taxAmount;
taxAmount=parseFloat(quantity) * parseFloat(rate)-subtotal;

        var totalamount = subtotal + taxAmount;

        var btnadd = "";
        var btnsub = "";


        //----for min stock------------
        minstockupdate(data, data.ItemID, itemunit);


        var htdata = "<div class='minstock_" + data.ItemID + '-' + itemunit + jcount + "'";
        if (data.KeepStock == true) {
            var qntmin = 0;
            if (data.ItemUnit == data.ItemUnitID) {
                qntmin = qunty * data.ConFactor;
            }
            if (data.ItemUnit == data.SubUnitId) {
                qntmin = qunty;
            }
            totalstock = data.total + qntmin;
            minstock = data.MinStock * data.ConFactor;
            htdata += " data-keeps='yes' data-min='" + minstock + "' data-confactor='" + data.ConFactor + "' data-stock='" + totalstock + "'>";
        }
        else {
            htdata += " data-keeps='no' >";
        }
        if ($(".minstock_" + data.ItemID + '-' + itemunit + jcount).length) {
            $(".minstock_" + data.ItemID + '-' + itemunit + jcount).remove();
        }
        //-----------------------------


        var inote = "";
        if (data) {
            inote = data.ItemNote;
        }
        var itemnote = '<div id="modal-item-' + count + '" class="modal fade" role="dialog" aria-hidden="true"><div class="modal-dialog"><div class="modal-content">' +
            '<div class="form-group"><textarea data-name="itemNote" name="itemnote" cols="40" rows="10" class="form-control inote" id="itemnote-' + count + '" maxlength="255">' + inote + '</textarea></div>' +
            '<div class="form-group text-center"><button class="btn btn-info" type="button" data-dismiss="modal">Save</button></div>' +
            '</div></div></div>';
        var notbtn = "<a class='itnote' data-name='ItName' data-toggle='modal' data-target='#modal-item-" + count + "'> " + data.Barcode + ' - ' + data.ItemName + "</a>";
        var itemValue = "<div class='item-name hidden' data-name='item-name'>" + data.ItemName + "</div>";

        var crowid = divid + '-' + itemunit + jcount;
        
        //-------------------------------------------------------------------------------------
        //$("#" + divid + '-' + itemunit+jcount).length == 0
        //if ($("#" + divid + '-' + itemunit + jcount).length == 0) {//$("#" + divid).length == 0 &&
        if (1==1) {//$("#" + divid).length == 0 &&
            var desc = "<span class='descr' data-name='Note'></span>";
            var exrow = "";
            if (data.type == "Bundle") {
                desc = "<br/>[<span class='descr' data-name='Note'>";
                $.each(datas.bundle, function (i, item) {
                    desc += item.ItemCode + " - " + item.ItemName;
                    desc += " - " + (item.quantity).toFixed(2) + " ";
                    desc += (item.ItemUnitName != null) ? item.ItemUnitName : "";
                    desc += "<br/>";
                });
                desc += "</span>]";
            }
            else if (data.Note != "" && data.Note != null && (typeof data.Note !== "undefined")) {
                desc = "<br/>[<span class='descr' data-name='Note'>" + data.Note + "</span>]";
            }

            //-----------item unit--------------------
            var selectunit = "";
            if (itemunit != null) {
                if (itemunit == data.ItemUnitID) {
                    // selectunit = "<select class='item_unit' id='item_unit_" + data.ItemID + "' data-name='ItemUnit' ><option value=" + data.ItemUnitID + ">" + data.PriUnit + "</option></select>";
                    selectunit = "<label>" + data.PriUnit + "</label><input type='hidden' class='item_unit' id='item_unit_" + data.ItemID + '-' + itemunit + "' data-name='ItemUnit' value=" + data.ItemUnitID + " />";
                }
                if (itemunit == data.SubUnitId) {
                    // selectunit = "<select class='item_unit' id='item_unit_" + data.ItemID + "' data-name='ItemUnit' ><option value=" + data.SubUnitId + ">" + data.SubUnit + "</option></select>";
                    selectunit = "<label>" + data.SubUnit + "</label><input type='hidden' class='item_unit' id='item_unit_" + data.ItemID + '-' + itemunit + "' data-name='ItemUnit' value=" + data.SubUnitId + " />";
                }
            }

            //------------------------------------------

            count++;
            var gtot = (data.price * qunty) + taxAmount;
            //var gtot = data.price + taxAmount;
            crowid = divid + '-' + itemunit + jcount;
            var btnadd = "<span id='" + data.ItemID + '-' + itemunit + jcount + "' class='input-group-btn qtyadd qtybtn hidden-xs'><i class='fa fa-1x fa-plus-circle' style='font-size:20px;line-height:30px;padding:3px'></i></span>";
            var btnsub = "<span id='" + data.ItemID + '-' + itemunit + jcount + "'class='input-group-btn qtysub qtybtn hidden-xs'><i class='fa fa-1x fa-minus-circle' style='font-size:20px;line-height:30px;padding:3px'></i></span>";
            var sizeoption;
            sizeoption = "<select name='itemsize' data-name='itemsize' class='sizeselect' onchange=getprice(this,'" + divid + '-' + itemunit + jcount + "') style='width:70px'>";
            sizeoption += "<option value=''></option>";
            var flag = 0;
            $.each(data.itemsize, function (i, item) {
                if ((data.Note == item.sizepriceid + "|" + item.price) || (data.Note == item.sizepriceid + "|" + item.price + ".00"))
                    sizeoption += "<option value='" + item.sizepriceid + "|" + item.price + "' selected>" + item.ItemSizeName + "</option>";

                else
                    sizeoption += "<option value='" + item.sizepriceid + "|" + item.price + "'>" + item.ItemSizeName + "</option>";
                flag = 1;
            });
            sizeoption += "</select>";
            var sizeoptions = "";
            if (flag == 1)
                sizeoptions = sizeoption
            rowdata = '<div class="price_main" id="' + divid + '-' + itemunit + jcount + '">' +
                '<input type="hidden" class="itemselect" data-name="Item" value="' + data.ItemID + '">' +
                '<div class="col-sm-1 hidden-xs"  >' + count + '</div>' + itemValue +
                '<div class="col-sm-4 col-xs-6">' + notbtn  + '</div>' +
                '<div class="col-sm-2  col-xs-1 hidden-xs" style="display:none">' +
                sizeoptions



                + '</div>' +
                /*    '<div class="col-sm-1">' + selectunit + '</div>' +*/
                //'<div class="col-sm-2"><div data-name="ItemUnitPrice" class="price">' + data.price.toFixed(2) + '</div></div>' +
                '<div class="col-sm-2 col-xs-2"><input type="text" style="width: 100%;height: 32px;" data-name="ItemUnitPrice" class="price numbers" value=' + data.price.toFixed(6) + ' id="' + divid + '-' + itemunit + jcount + '"></div>' +
                '<div class="col-sm-3 col-xs-3"><div class="input-group input-group-sm">' + btnsub + '<input name="price_q" id="qty_' + data.ItemID + '-' + itemunit + jcount + '" data-name="ItemQuantity" type="text" class="price_q" value=' + qunty + ' onchange="qtychange(this,' + data.ItemID + ')";>' + btnadd + '</div></div>' +
                '<input data-name="ItemTax" type="hidden" class="taxper" value="' + data.Tax.toFixed(2) + '"><input data-name="BasePrice" type="hidden" id="base_rate_' + data.ItemID + jcount + '" value="' + data.BasePrice + '"><input data-name="ConFactor" type="hidden" id="cfactor_' + data.ItemID + '" value="' + data.ConFactor + '">' +
                '<div class="col-sm-1 col-xs-1 hidden-xs"><input readonly data-name="ItemTaxAmount" type="text" class="taxamt class="hidden-xs"" value="' + taxAmount.toFixed(2) + '" style="width: 100%;text - align: center;height: 32px;"></div>' +
                '<div class="col-sm-1 col-xs-1"><div data-name="ItemSubTotal" class="subtot hidden">' + (subtotal).toFixed(2) + '</div><div data-name="Itemgtotal" class="gtotal">' +(subtotal+taxAmount).toFixed(2) + '</div> </div>' +
                '<div class="col-sm-1 col-xs-1"><i class="fa fa-trash-o rowtrash" title="Remove"></i></div>' +
                '<div class="itdisc hidden" data-name="ItemDiscount">0.00</div>' + itemnote + htdata //added for min stock
            '</div>';
            $('#POSForm').append(rowdata);
            j++;
        }
        else {
            qunty = parseFloat(qunty) || 1;
            qty = parseFloat($('#' + divid + '-' + itemunit + jcount).find("input.price_q").val());
            qty += qunty;
            $('#' + divid + '-' + itemunit + jcount).find("input.price_q").val(qty.toFixed(2));
            rowSubTotal('rowitem_' + data.ItemID + '-' + itemunit + jcount);
        }

        $("#itemcount").text($('.price_main').length);
        if (stype == null) {
            $('#' + divid + '-' + itemunit).find("input.price_q").focus();
        }
        else if (stype == "scan") {
            $("#barcode").focus();
        }
        itemTotal(crowid);
        var disc_p = parseFloat($("#disc_p").val()) || 0;
        if (disc_p != 0) {
            discAmount();
        }
        else {
            var disc_m = parseFloat($("#disc_m").val()) || 0;
            if (disc_m != 0) {
                discPerc();
            }
        }
        //rowSubTotal('rowitem_' + data.ItemID + '-' + itemunit);
        // check touck keyboard enable/disable
        displaynumpad();
    } else {
        alert("This Item is Out of Stock!!!");
    }
    $("#txtamounttopay").val($("#total_payable").val());
}
function getprice(obj, divid) {
    var sp = $(obj).val().split("|");

    $("#" + divid + " .price").val(sp[1]);
    rowSubTotal(divid);

}
// create row for weight item
// create row for weight item
function createRowWeigh(data) {

    var divid = "rowitem_" + data.id;
    var qunty = parseFloat(data.Weight.toFixed(3));
    var quantity = 1;
    var rate = parseFloat(data.price);
    var subtotal = rate * qunty;
    subtotal = subtotal.toFixed(3);
    subtotal = parseFloat(subtotal).toFixed(2);
    var tax = parseFloat(data.Tax);
    var taxAmount = parseFloat(subtotal) * (parseFloat(tax) / 100);
    var totalamount = subtotal + taxAmount;
    var itemunit = data.ItemUnitID;
    var btnadd = "<span id='" + data.ItemID + '-' + itemunit + jcount + "' class='input-group-btn qtyadd qtybtn'><i class='fa fa-1x fa-plus-circle'></i></span>";
    var btnsub = "<span id='" + data.ItemID + '-' + itemunit + jcount + "'class='input-group-btn qtysub qtybtn'><i class='fa fa-1x fa-minus-circle'></i></span>";


    //----for min stock------------
    minstockupdate(data, data.ItemID, itemunit);
    var htdata = "<div class='minstock_" + data.ItemID + '-' + itemunit + "'";
    if (data.KeepStock == true) {
        var qntmin = 0;
        if (data.ItemUnit == data.ItemUnitID) {
            qntmin = qunty * data.ConFactor;
        }
        if (data.ItemUnit == data.SubUnitId) {
            qntmin = qunty;
        }
        totalstock = data.total + qntmin;
        minstock = data.MinStock * data.ConFactor;
        htdata += " data-keeps='yes' data-min='" + minstock + "' data-confactor='" + data.ConFactor + "' data-stock='" + totalstock + "'>";
    }
    else {
        htdata += " data-keeps='no' >";
    }
    if ($(".minstock_" + data.ItemID + '-' + itemunit).length) {
        $(".minstock_" + data.ItemID + '-' + itemunit).remove();
    }
    //-----------------------------


    var inote = "";
    if (data) {
        inote = data.ItemNote;
    }
    var itemnote = '<div id="modal-item-' + count + '" class="modal fade" role="dialog" aria-hidden="true"><div class="modal-dialog"><div class="modal-content">' +
        '<div class="form-group"><textarea data-name="itemNote" name="itemnote" cols="40" rows="10" class="form-control inote" id="itemnote-' + count + '" maxlength="255">' + inote + '</textarea></div>' +
        '<div class="form-group text-center"><button class="btn btn-info" type="button" data-dismiss="modal">Save</button></div>' +
        '</div></div></div>';
    var notbtn = "<a class='itnote' data-name='ItemsName' data-toggle='modal' data-target='#modal-item-" + count + "'> " + data.ItemCode + ' - ' + data.ItemName + "</a>";
    var itemValue = "<div class='item-name hidden' data-name='item-name'>" + data.ItemName + "</div>";

    var crowid = divid + '-' + itemunit;
    //-------------------------------------------------------------------------------------

    if ($("#" + divid + '-' + itemunit).length == 0) {//$("#" + divid).length == 0 && 
        var desc = "<span class='descr' data-name='Note'></span>";
        var exrow = "";
        if (data.Note != "" && data.Note != null && (typeof data.Note !== "undefined")) {
            desc = "<br/>[<span class='descr' data-name='Note'>" + data.Note + "</span>]";
        }

        //-----------item unit--------------------
        var selectunit = "";
        if (itemunit != null) {
            if (itemunit == data.ItemUnitID) {
                selectunit = "<label>" + data.PriUnit + "</label><input type='hidden' class='item_unit' id='item_unit_" + data.ItemID + '-' + itemunit + "' data-name='ItemUnit' value=" + data.ItemUnitID + " />";
            }
            if (itemunit == data.SubUnitId) {
                selectunit = "<label>" + data.SubUnit + "</label><input type='hidden' class='item_unit' id='item_unit_" + data.ItemID + '-' + itemunit + "' data-name='ItemUnit' value=" + data.SubUnitId + " />";
            }
        }
        //------------------------------------------

        count++;
        var gtot = parseFloat(subtotal) + parseFloat(taxAmount);
        rowdata = '<div class="price_main" id="' + divid + '-' + itemunit + '">' +
            '<input type="hidden" class="itemselect" data-name="Item" value="' + data.ItemID + '">' +
            '<div class="col-sm-1">' + count + '</div>' + itemValue +
            '<div class="col-sm-3">' + notbtn + desc + '</div>' +
            '<div class="col-sm-1">' + selectunit + '</div>' +
            '<div class="col-sm-1"><input type="text" style="width: 100%;height: 32px;" data-name="ItemUnitPrice" class="price" value=' + data.price.toFixed(2) + ' id="' + divid + '-' + itemunit + '"></div>' +
            '<div class="col-sm-2"><div class="input-group input-group-sm">' + btnsub + '<input name="price_q" id="qty_' + data.ItemID + '-' + itemunit + '" data-name="ItemQuantity" type="text" class="price_q" value=' + qunty.toFixed(3) + ' onchange="qtychange(this,' + data.ItemID + ')";>' + btnadd + '</div><input data-name="ItemTax" type="hidden" class="taxper" value="' + data.Tax.toFixed(2) + '"><input data-name="BasePrice" type="hidden" id="base_rate_' + data.ItemID + '" value="' + data.BasePrice + '"><input data-name="ConFactor" type="hidden" id="cfactor_' + data.ItemID + '" value="' + data.ConFactor + '"></div>' +
            '<div class="col-sm-1"><input data-name="ItemTaxAmount" type="text" readonly class="taxamt" value="' + taxAmount.toFixed(2) + '"  style="width:100%;border:0px;text-align: center;""></div>' +
            '<div class="col-sm-2"><div data-name="ItemSubTotal" class="subtot hidden">' + subtotal + '</div><div data-name="Itemgtotal" class="gtotal">' + gtot.toFixed(2) + '</div></div>' +
            '<div class="col-sm-1"><i class="fa fa-trash-o rowtrash" title="Remove"></i></div>' +
            '<div class="itdisc hidden" data-name="ItemDiscount">0.00</div>' + itemnote + htdata + '</div>';
        $('#POSForm').append(rowdata);
    }
    else {
        qunty = parseFloat(qunty) || 1;
        qty = parseFloat($('#' + divid + '-' + itemunit).find("input.price_q").val());
        qty += qunty;
        $('#' + divid + '-' + itemunit).find("input.price_q").val(qty.toFixed(2));
        rowSubTotal('rowitem_' + data.ItemID + '-' + itemunit);
    }
    $("#barcode").focus();
    itemTotal(crowid);
    var disc_p = parseFloat($("#disc_p").val()) || 0;
    if (disc_p != 0) {
        discAmount();
    }
    else {
        var disc_m = parseFloat($("#disc_m").val()) || 0;
        if (disc_m != 0) {
            discPerc();
        }
    }
    displaynumpad();
}


// check touck keyboard enable/disable
function displaynumpad() {
    //$(".price_q").keypad({
    //    target: $('.inlineTarget:first'),
    //    keypadOnly: false,
    //    separator: '|',
    //    deltText: 'BACK',
    //    deltStatus: 'BACK SPACE',
    //    layout: ['7|8|9|' + $.keypad.CLOSE,
    //    '4|5|6|' + $.keypad.DELT,
    //    '1|2|3|' + $.keypad.CLEAR,
    //        '.|0|00'
    //    ]
    //});
    //$(".price").keypad({
    //    target: $('.inlineTarget:first'),
    //    keypadOnly: false,
    //    separator: '|',
    //    deltText: 'BACK',
    //    deltStatus: 'BACK SPACE',
    //    layout: ['7|8|9|' + $.keypad.CLOSE,
    //    '4|5|6|' + $.keypad.DELT,
    //    '1|2|3|' + $.keypad.CLEAR,
    //        '.|0|00'
    //    ]
    //});

}
function qtychange(selectObject, arg) {
    minstockcheck(arg);
}
//function unitchange(selectObject, arg) {
//    minstockcheck(arg);
//    var index = $('#item_unit_' + arg).prop('selectedIndex');
//    if (index == 1) {
//        var unitId = parseFloat($('#item_unit_' + arg).val());
//        var cfactor = parseFloat($('#cfactor_' + arg).val());
//        var price = parseFloat($("#base_rate_" + arg).val());
//        var newprice = parseFloat(price / cfactor);
//        $('#rowitem_' + arg).find(".price").text(newprice.toFixed(2));
//    } else {
//        var unitId = parseFloat($('#item_unit_' + arg).val());
//        var price = parseFloat($("#base_rate_" + arg).val());
//        $('#rowitem_' + arg).find(".price").text(price.toFixed(2));
//    }
//    rowSubTotal('rowitem_' + arg);
//}

//check minimum stock
function minstockupdate(result, dataid, itemunit) {
    var htdata = "<div class='minstock_" + dataid + '-' + itemunit + "'";
    if (result.KeepStock == true) {
        totalstock = result.total;
        minstock = result.MinStock * result.ConFactor;

        htdata += " data-keeps='yes' data-min='" + minstock + "' data-confactor='" + result.ConFactor + "' data-stock='" + totalstock + "'>";
    }
    else {
        htdata += " data-keeps='no' >";
    }
    if ($(".minstock_" + dataid + '-' + itemunit).length) {
        $(".minstock_" + dataid + '-' + itemunit).remove();
    }
    $('#rowitem_' + dataid + '-' + itemunit).append(htdata);
}


// add quantity click button function
$(document).on('click', '.qtyadd', function () {
    var Id = $(this).attr('id');
    minstockcheck(Id);
    var qty = parseFloat($("#qty_" + Id).val());
    var qtyno = (qty + 1);
    if (qtyno >= 1) {
        $("#qty_" + Id).val(qtyno);
    }
    rowSubTotal('rowitem_' + Id);
});

// less quantity click button function
$(document).on('click', '.qtysub', function () {
    var Id = $(this).attr('id');
    minstockcheck(Id);
    var qty = parseFloat($("#qty_" + Id).val());
    var qtyno = (qty - 1);
    if (qtyno >= 1) {
        $("#qty_" + Id).val(qtyno);
    }
    rowSubTotal('rowitem_' + Id);
});
$(document).on('change', '.price', function (e) {
    var Id = $(this).attr('id');
    var amt=$(this).val();
   var dummyv = parseFloat(parseFloat(amt) / parseFloat(1 + parseFloat(5 / 100))).toFixed(6);
   $(this).val(dummyv);
    rowSubTotal(Id);
});

// calculate subtotal on row on quantity change

function rowSubTotal(arg) {

    var quantity = $("#" + arg + " .price_q").val();
    var rate = $("#" + arg + " .price").val();
    quantity = quantity || 0;
    rate = rate || 0;
    var subtotal = (parseFloat(quantity) * parseFloat(rate));
    $('#' + arg).find(".subtot").text(subtotal.toFixed(2));
    var tax = $("#" + arg + " .taxper").val();
    var taxAmount = parseFloat(subtotal) * (parseFloat(tax) / 100);
    $('#' + arg).find(".taxamt").val(taxAmount.toFixed(2));

    var gtotal = subtotal + taxAmount;
    $('#' + arg).find(".gtotal").text(gtotal.toFixed(2));

    itemDisc();
    itemTotal(arg);
}
function minstockcheck(arg) {
    var keepstock = $(".minstock_" + arg).attr('data-keeps');
    if (keepstock == "yes") {

        var index = $('#item_unit_' + arg).prop('selectedIndex');
        var unitname = $('#item_unit_' + arg).find('option:selected').text();
        var minstock = parseFloat($(".minstock_" + arg).attr('data-min'));
        var confactor = parseFloat($(".minstock_" + arg).attr('data-confactor'));
        var stock = parseFloat($(".minstock_" + arg).attr('data-stock'));
        var quantity = parseFloat($("#qty_" + arg).val());
        var qty = 0;
        var classn = $("#rowitem_" + arg).attr('class');

        $("." + classn).each(function () {
            var rowid = $(this).attr('id');
            var arr = rowid.split('_');
            var arg1 = arr[1];
            var index1 = $("#" + rowid + " .item_unit").prop('selectedIndex');
            var curent = $("#" + rowid + " .price_q").val();
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
                $("#qty_" + arg).val(parseInt(stock));
            }
        } else {
            stock = stock - (qty - quantity);
            var totstock = stock - quantity;
            if (totstock <= minstock && totstock >= 0) {
                alert("Stock Exceeds Minimum Stock");
            }
            if (totstock < 0) {
                alert("This Item Is Going To Out of Stock!!! Only " + stock + " " + unitname + " Items Are Available In Stock..");
                $("#qty_" + arg).val(stock);
            }
        }
    }
}

// calculate iem wise discount and also update tax based on discount
function itemDisc() {
    if ($('#tax_disc').prop('checked') == true) {
        var disc_p = parseFloat($("#disc_p").val()) || 0;
        if ($('.price_main').length > 0) {
            $('.price_main').each(function (rowIndex, r) {
                var subtotal = parseFloat($(this).find(".subtot").text());
                var Pe_amount = (subtotal * disc_p) / 100;
                $(this).find('.itdisc').val(Pe_amount);

                subtotal -= Pe_amount;
                var tax = parseFloat($(this).find(".taxper").val());
                var taxAmount = parseFloat((subtotal * tax) / 100) || 0;
                $(this).find(".taxamt").val(taxAmount.toFixed(2));
            });
        }
    }
    else {
        if ($('.price_main').length > 0) {
            $('.price_main').each(function (rowIndex, r) {
                var subtotal = parseFloat($(this).find(".subtot").text());

                var tax = parseFloat($(this).find(".taxper").val());
                var taxAmount = parseFloat((subtotal * tax) / 100) || 0;
                $(this).find(".taxamt").val(taxAmount.toFixed(2));
            });
        }
    }
}
function itemTotalres() {
    //alert("hi");
    var MainClass = '#POSForm';
    if ($("#leftCustomItem").hasClass("cust-active")) {
        MainClass = '#leftCustomItem';
        MainClass = '#leftCustomItem';
    }

    var itemCount = $(MainClass + ' .price_main').length;
    var itemQty = 0;
    $(MainClass + ' .price_q').each(function () {
        itemQty += parseFloat($(this).val());
    });
    var totalextax = 0;
    $(MainClass + ' .subtot').each(function () {
        totalextax += parseFloat($(this).text());
    });
    var totalTax = 0;
    if (MainClass == '#POSForm') {
        $(MainClass + ' .taxamt').each(function (ind) {
            totalTax += parseFloat($(this).val());
        });
        var discount = parseFloat($('#discount').val()) || 0;
        var total = (totalextax + totalTax) - discount;
        $("#ItemCount").val(itemCount);
        $("#ItemCounts").text(itemCount);
        $("#ItemQty").val(itemQty.toFixed(2));
        $("#total").val(totalextax.toFixed(2));
        $("#total_tax").val(totalTax.toFixed(2));
        $("#total_payable").val(total.toFixed(2));
        $("#total_payables").text(total.toFixed(2));
        $("#quick-payable").text(total.toFixed(2));
        $('#discount').val(discount.toFixed(2));
        cashBalance();
    }
    else {
        // var discount = parseFloat($('#discount').val()) || 0;
        //var total = (totalextax + totalTax) - discount;
        var taxP = parseFloat($('#Custom-total_tax_p').val()) || 0;
        totalTax = parseFloat(totalextax) * (parseFloat(taxP) / 100);
        var total = (totalextax + totalTax);
        $("#Custom-ItemCount").val(itemCount);
        $("#Custom-ItemQty").val(itemQty.toFixed(2));
        $("#Custom-total").val(totalextax.toFixed(2));
        $("#Custom-total_tax").val(totalTax.toFixed(2));
        $("#Custom-total_payable").val(total.toFixed(2));

        //$('#Custom-discount').val(discount.toFixed(2));

    }
}
// calculate total value
function itemTotal(arg) {
    var itemCount = $('.price_main').length;
    var itemQty = 0;
    $('.price_q').each(function () {
        itemQty += parseFloat($(this).val());
    });
    var totalextax = 0;
    $('.subtot').each(function () {
        totalextax += parseFloat($(this).text());
    });
    var totalTax = 0;
    $('.taxamt').each(function () {
        totalTax += parseFloat($(this).val());
    });
    totalTax = getRoundOff(totalTax);
    totalTax = parseFloat(totalTax);
    var discount = 0;
    var roundoff = 0;

    discount = parseFloat($(".disc-am").val());
    var ldelivary = parseFloat($("#dcharge").val());
    if (Number.isNaN(ldelivary))
        ldelivary = 0;
    var delchargetax = ldelivary * 5 / 100;
    delchargetax = getRoundOff(delchargetax);
    delchargetax = parseFloat(delchargetax);
    var dicounttax = 0;//discount * 5 / 100;

    dicounttax = 0;//getRoundOff(dicounttax);
    dicounttax =0;// parseFloat(dicounttax);
    if (Number.isNaN(totalTax))
        totalTax = 0;
    if (Number.isNaN(delchargetax))
        delchargetax = 0;
    if (Number.isNaN(dicounttax))
        dicounttax = 0;

    totalTax = totalTax + delchargetax - dicounttax;
    roundoff = parseFloat($(".round-Off").val());
    if (Number.isNaN(discount))
        discount = 0
    if (Number.isNaN(roundoff))
        roundoff = 0;
    if (Number.isNaN(ldelivary))
        ldelivary = 0;


    var netamount = totalextax + totalTax - discount;

    var total = (totalextax + totalTax + ldelivary) - (discount + roundoff);


    $("#ItemCount").val(itemCount);
    $("#ItemCounts").text(itemCount);
    $("#ItemQty").val(itemQty.toFixed(2));

    $(".subtotal").val(totalextax.toFixed(2));
    $("#total_tax").val(totalTax.toFixed(2));
    $("#total_payable").val(total.toFixed(2));
    $("#total_payables").text(total.toFixed(2));
    $("#total").val(total.toFixed(2));
    $("#quick-payable").text(total.toFixed(2));
    $(".round-Off").val(roundoff.toFixed(2));
    $(".net-amount").val(netamount.toFixed(2));
    cashBalance();
    var crowid = (typeof arg === "undefined") ? "" : arg;
    //DisplayBoard(crowid);
}

function itemlist(data) {
    var datas = "";
    var Menu = '<div id="itscrol" style="height:100% !important; width: 25%;height: calc(100vh)!important;overflow: scroll; "><ul id="filters">';
    var topMenu = "";
    var proditems = '<div id="portfoliolist">';
    var category = "";
    var myclass = "";
    var Addon = "";
    var Addonitem = "";
    $.each(data, function (i, item) {
        count = i + 1;
        prod = '';
        if (item.ItemCategoryID != category) {
            category = item.ItemCategoryID;
            myclass = 'items_' + item.ItemCategoryID;
            topMenu += '<li><span class="filter" data-filter=".' + myclass + '">' + item.Category + '</span></li>';
        }
        if (item.ImageFile != null) {
            itemimage = item.ItemID + '/' + item.ImageFile;
        }
        else {
            itemimage = "default.jpg";
        }
        if (item.Topping == true) {
            if (Addon == "") {
                Addon = '<li><span class="filter" data-filter=".addonss">Addons</span></li>';
            }
            Addonitem = '<div class="portfolio addonss" data-cat="addonss" data-id="' + item.ItemID + '">' +
                '<div class="portfolio-wrapper">' +
                '<div class="gallery">' +
                
                '<div class="pricetag">' +
                '<span class="badge">' + item.SellingPrice.toFixed(2) + '</span>' +
                //'<span class="btn btn-success btn-xs">$300</span>' +
                '</div>' +
                '<div class="middle">' +
                '<div class="text-note"><h5>' + item.ItemName + '</h5>' +
                '</div>' +
                '</div>' +
                '</div>' +
                '</div>' +
                '</div>';
        }
        prod = '<div class="portfolio ' + myclass + '" data-cat="' + myclass + '" data-id="' + item.ItemID + '">' +
            '<div class="portfolio-wrapper">' +
            '<div class="gallery">' +
            '<br/><hr/>' +
            '<div class="pricetag">' +
            '<span class="badge">' + item.SellingPrice.toFixed(2) + '</span>' +
            //'<span class="btn btn-success btn-xs">$300</span>' +
            '</div>' +
            '<div class="middle">' +
            '<div class="text-note"><h5>' + item.ItemName + '</h5>' +
            '</div>' +
            '</div>' +
            '</div>' +
            '</div>' +
            '</div>';
        proditems += prod;

    })
    allclass = '<li><span class="filter active" data-filter=":not(.addonss)">All</span></li>';
    Menu += allclass + " " + topMenu + Addon + "</ul></div>";
    proditems += Addonitem + "</div>";
    datas = Menu + proditems;
    return datas;
}
function tablelist() {
    $.ajax({
        async: true,
        cache: false,
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/Table/AllTables",
        data: { id: '' },
        success: function (item) {
            $('#tablelist').html("");
            $('#tablelist').append(alltableist(item));
            try {
                filterData.del('#tableportfoliolist');
            } catch (x) { }
            filterData.init('#tableportfoliolist', '.pftable', '.fltrtable');
        }
    });
}
$(document).on('click', '#tablesubmit', function (event) {
    var pplcount = $('#lblpeoplcount').val();
    if (pplcount == "") {
        alert("Please Enter People Count..");
        $('#lblpeoplcount').val("");
        // $('#dineInModel').modal('show');
    } else if (pplcount <= 0) {
        alert("Please Enter Valid Number");
        $('#lblpeoplcount').val("");
    }
    else {
        var tble = $("#lblTable").text();
        $("#dinein").text("Dine in on " + tble);
        $('#dinein').addClass("btn-primary");
        $('#dinein').removeClass("btn-warning");
        $('#dineInModel').modal('hide');
        $('#opencart').modal('hide');
        
        setTimeout(function () {
             
            
            $("#opencart").modal("show");
        }, 1000);
    }
});

function alltableist(data) {
    if (data != "") {
        $("#tabledetail").hide();

        var datas = "";
        var Menu = '<div id="tblscrol"><ul id="filterstable" class="filter-head">';
        var topMenu = "";
        var proditems = '<div id="tableportfoliolist">';
        var area = "";
        var myclass = "";
        $.each(data, function (i, item) {
            count = i + 1;
            prod = '';
            if (item.AreaId != area) {
                area = item.AreaId;
                myclass = 'area_' + item.AreaId;
                // topMenu += '<li><span class="fltrtable" data-filter=".' + myclass + '">' + item.Area + '</span></li>';
            }
            itemimage = "";
            if (item.TableStatus == 0) {
                //in use
                myclass += " bg-orange";
            }
            if (item.TableStatus == 1) {
                //out of use
                myclass += " bg-red";
            }
            if (item.TableStatus == 2) {
                //available
                myclass += " bg-green";
            }
            if (item.TableStatus == 3) {
                //reserved
                myclass += " bg-blue";
            }

            prod = '<div class="pftable ' + myclass + '" data-cat="' + myclass + '" id="table_' + item.TableId + '" data-id="' + item.TableId + '">' +
                '<div class="pftable-wrapper">' +
                '<div class="gallery">' +
                '<div class="images"></div>' +
                //'<img src="/uploads/itemimages/' + itemimage + '"  class="images">' +
                '<div class="top">' +
                '<div class="text-note"><h5>' + item.TableName + '</h5>' +
                '<p><strong>Seats</strong></p>' +
                '<p><span class="pull-left">Maximum: ' + item.MaxSeats + '</span><span class="pull-right">Used: ' + item.usedSeat + '</span></p>' +
                '<p>' +
                '<i>' + item.CuStatus + '</i>' +
                '</p>' +
                '</div>' +
                '</div>' +
                '</div>' +
                '</div>' +
                '</div>';
            proditems += prod;
        })
        allclass = '<li><span class="fltrtable active" data-filter="all">All</span></li>';
        Menu += allclass + " " + topMenu + "</ul></div>";
        proditems += "</div>";
        datas = Menu + proditems;
        return datas;
    } else {
        $('#dineInModel').modal("hide");
    }
}

function tableDetails(dataid) {
    if (dataid != null) {
        $("#tabledetail").show();
        $.ajax({
            url: '/Table/getTableById',
            type: "GET",
            dataType: "JSON",
            data: { TableId: dataid },
            success: function (result) {
                $("#peoplecount").hide();
                $("#seatused").hide();


                $("#lbltableval").text(result.TableId);
                $("#lblTable").text(result.TableName);
                $("#lblmaxseat").text(result.MaxSeats);
                $("#lblusedseat").text(result.usedSeat);
                var status = "";
                if (result.TableStatus == 0) {
                    // status = "In Use";
                    $("#seatused").show();
                    if (result.MaxSeats > result.usedSeat) {
                        $("#peoplecount").show();
                    }
                    // $('#table_' + result.TableId).css({ "background-color": "orange" });
                }
                if (result.TableStatus == 1) {
                    status = "Out of Use";
                }
                if (result.TableStatus == 2) {
                    // status = "Available";
                    $("#seatused").show();
                    $("#peoplecount").show();

                    // $('#table_' + result.TableId).css({ "background-color": "green" });


                    //$("#dinein").addClass('btn btn-primary');
                }
                if (result.TableStatus == 3) {
                    status = "Reserved";
                }
                if (result.TableStatus == 0 || result.TableStatus == 2) {
                    if (result.MaxSeats > result.usedSeat) {
                        if (result.usedSeat == 0) {
                            status = "Available";
                        } else {
                            status = "In Use";
                        }
                    }
                }

                $("#lblstatus").text(status);
            }
        });
    }
}
// balance amount calculation 
function cashBalance() {
    var totalPayable = parseFloat($("#total_payable").val());
    var totalPaid = parseFloat($("#amount").val()) || 0;

    var balance = parseFloat(totalPaid) - parseFloat(totalPayable);
    balance = parseFloat(balance) || 0
    $("#total_paying").text(parseFloat(totalPaid).toFixed(2));
    $("#balance").text(balance.toFixed(2));

}

// reset entries function
function resetfiled() {
    $("#myposforms")[0].reset();
    $("div.price_main").remove();
    $("#ddlCustomer").val(null).trigger("change");
    $("#ddlSalesPerson").val(null).trigger("change");
    itemTotal();
}

function resetfiledres() {
    $("#myposforms")[0].reset();
    $(".price_main").remove();
    $("#ddlCustomer").val(null).trigger("change");
    $("#ddlSalesPerson").val(null).trigger("change");
    $('#HideOrderId').val("");
    $("#itemcount").text("0");
    $("#takeaway").click();
    $("#discount").val(0);
    $(".disc-am").val(0);
    itemTotalres();
}


////check min stock
//function minstockcheck(arg) {
//    var keepstock = $(".minstock_" + arg).attr('data-keeps');
//    if (keepstock == "yes") {
//        var index = $('#unit_name_' + arg).prop('selectedIndex');
//        var unitname = $('#unit_name_' + arg).find('option:selected').text();
//        var minstock = parseFloat($(".minstock_" + arg).attr('data-min'));
//        var confactor = parseFloat($(".minstock_" + arg).attr('data-confactor'));
//        var stock = parseFloat($(".minstock_" + arg).attr('data-stock'));
//        var quantity = parseFloat($(".total_qntt_" + arg).val());
//        var qty = 0;
//        var classn = $("#item_" + arg).attr('class');
//        $("." + classn).each(function () {
//            var rowid = $(this).attr('id');
//            var arr = rowid.split('_');
//            var arg1 = arr[1];
//            var index1 = $("#" + rowid + " .units").prop('selectedIndex');
//            var curent = $("#" + rowid + " .quty").val();
//            var confactor1 = parseFloat($("#" + rowid + "  .minstock_" + arg1).attr('data-confactor'));
//            if (index == 0) {
//                qty += (curent * confactor1);
//            }
//            else {
//                qty += curent;
//            }
//        });
//        if (index == 0) {
//            stock = stock - (qty - quantity);
//            minstock = minstock / confactor;
//            stock = stock / confactor;
//            var tostock = stock - quantity;
//            var totstock = tostock / confactor;

//            //var totstock = stock - qty;
//            if (totstock <= minstock && totstock >= 0) {
//                alert("Stock Exceeds Minimum Stock");
//            }
//            if (totstock < 0) {
//                stock = stock.toFixed(2);
//                alert("This Item Is Going To Out of Stock!!! Only " + stock + " " + unitname + "Items Are Available In Stock..");
//                $(".total_qntt_" + arg).val(parseInt(stock));
//            }
//        } else {
//            stock = stock - (qty - quantity);
//            var totstock = stock - quantity;
//            if (totstock <= minstock && totstock >= 0) {
//                alert("Stock Exceeds Minimum Stock");
//            }
//            if (totstock < 0) {
//                alert("This Item Is Going To Out of Stock!!! Only " + stock + " " + unitname + " Items Are Available In Stock..");
//                $(".total_qntt_" + arg).val(stock);
//            }
//        }
//    }
//}



// form submit function
function FormSubmit(fnval) {
    var url = $('#myposforms')[0].action;
    var taxptot = 0;
    var itemcount = parseFloat($('#ItemCount').val());
    $('#POSForm .taxper').each(function () {
        taxptot += parseFloat($(this).val());
    });
    var taxper = (taxptot / itemcount).toFixed(2);
    var HTMLtbl = {
        getData: function (divid) {
            var data = [];
            divid.find('.price_main').each(function (rowIndex, r) {
                var cols = {};
                $(this).find('input,textarea,select,.price,.subtot,.descr,.inote').each(function (colIndex, c) {
                    itid = $(this).attr('data-name').split(' ')[0];
                    itval = ($(this).val() != "") ? $(this).val() : $(this).text();
                    cols[itid] = itval;
                });
                data.push(cols);
            });
            return data;
        }
    }


    var salesEntry = {
        'SENo': $('#SENo').val(),
        'BillNo': $('#OrderNo').val(),
        'SEDate': $('#SEDate').val(),
        'SECashier': ($("#HideWaiterId").val() != null) ? $("#HideWaiterId").val(): $('#ddlSalesPerson').val(),
        'Customer': $('#ddlCustomer').val(),
        'CustomerType': $('#CustomerType').val(),
        "PayType": $('#PayMethod').val(),
        'SEItems': $('#ItemCount').val(),
        'SEItemQuantity': $('#ItemQty').val(),
        'SESubTotal': $('#total').val(),
        'SETax': taxper,
        'SETaxAmount': $('#total_tax').val(),
        'SEDiscount': $('#discounts').val(),
        'SEGrandTotal': $('#total_payable').val(),
        'SENote': $('#SENote').val(),
        'taxAFdisc': $('#tax_disc').prop('checked'),
        'OrderRefer': $('#OrderId').val(),

    }

    if (fnval == "payprint") {
        var posData = {
            'PayMethod': "Cash",
            'TotTender': $('#total_payable').text(),
            'ChangeDue': "0.00",
            'PayMode': "",
        }
        var salePayment = {
            'SEPaidAmount': $('#total_payable').val()
        }
        fnval = "print";
    } else {
        var posData = {
            'PayMethod': $('#PayMethod').val(),
            'TotTender': $('#total_paying').text(),
            'ChangeDue': $('#balance').text(),
            'PayMode': $('#PayMode option:selected').text(),
        }
        var salePayment = {
            'SEPaidAmount': $('#amount').val()
        }
    }

    var wCustomer = {
        'CustomerName': $('#CustomerName').val(),
        'MobileNo': $('#MobileNo').val()
    }



    var data = HTMLtbl.getData($('#POSForm'));



    var parameters = {};
    parameters.saledata = salesEntry;
    parameters.seItems = data;
    parameters.SEDate = $('#SEDate').val();
    parameters.salePayment = salePayment;
    parameters.posData = posData;
    parameters.fnval = fnval;
    parameters.wCustomer = wCustomer;
    parameters.roundoff = $('#round-Off').val();
    parameters.istax = $('#tax_disc').prop('checked');
    console.log(parameters);
    $.ajax({
        async: true,
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

                    BindBill(e);
                    window.location = "/POS/Create";
                } else {
                    //  $('.ajax_response', res_success).text(e.message);
                    //  $('.AlertDiv').prepend(res_success);
                }
                resetfiledres();
                $("button").prop('disabled', false);
                //setInterval(window.location.href = '/POS/Create', 120);
                if ($("#OrderNo").val() == e.billno) {


                    // $("#OrderNo").val(orderno);
                    $("#OrderNo").val(parseInt($("#OrderNo").val()) + 1)
                }
                else {
                    $("#OrderNo").val(parseInt(e.billno) + 1)
                }

            } else {

                // $('.ajax_response', res_danger).text(e.message);
                //  $('.AlertDiv').prepend(res_danger);
                $("button").prop('disabled', false); // enable button

                //var temp = $('body').html();
                //$('body').html(printContent);
                //$('body').css('padding-right', '0px');

                //window.print();
                //$('body').html(temp);

            }
            $("button").prop('disabled', false);
        }
    });
}


//edit
function EditFormSubmit(fnval) {
    var url = $('#myposforms')[0].action;
    var taxptot = 0;
    var itemcount = parseFloat($('#ItemCount').val());
    $('#POSForm .taxper').each(function () {
        taxptot += parseFloat($(this).val());
    });
    var taxper = (taxptot / itemcount).toFixed(2);
    var HTMLtbl = {
        getData: function (divid) {
            var data = [];
            divid.find('.price_main').each(function (rowIndex, r) {
                var cols = {};
                $(this).find('input,textarea,select,.price,.subtot,.descr,.inote').each(function (colIndex, c) {
                    itid = $(this).attr('data-name').split(' ')[0];
                    itval = ($(this).val() != "") ? $(this).val() : $(this).text();
                    cols[itid] = itval;
                });
                data.push(cols);
            });
            return data;
        }
    }

    var salesEntry = {
        'SENo': $('#SENo').val(),
        'BillNo': $('#BillNo').val(),
        'SEDate': $('#SEDate').val(),
        'SECashier': $('#SECashier').val(),
        'Customer': $('#ddlCustomer').val(),
        'CustomerType': $('#CustomerType').val(),
        "PayType": $('#PayMethod').val(),

        'SEItems': $('#ItemCount').val(),
        'SEItemQuantity': $('#ItemQty').val(),
        'SESubTotal': $('#total').val(),
        'SETax': taxper,
        'SETaxAmount': $('#total_tax').val(),
        'SEDiscount': $('.disc-am').val(),
        'SEGrandTotal': $('#total_payable').val(),
        'SENote': $('#SENote').val(),
        'taxAFdisc': $('#tax_disc').prop('checked'),
        'OrderRefer': $('#OrderId').val(),
    }

    var posData = {
        'PayMethod': $('#PayMethod').val(),
        'TotTender': $('#total_paying').text(),
        'ChangeDue': $('#balance').text(),
        'PayMode': $('#PayMode option:selected').text(),
    }
    var wCustomer = {
        'CustomerName': $('#CustomerName').val(),
        'MobileNo': $('#MobileNo').val()
    }
    var salePayment = {
        'SEPaidAmount': $('#amount').val()
    }
    var data = HTMLtbl.getData($('#POSForm'));
    var parameters = {};
    parameters.saledata = salesEntry;
    parameters.seItems = data;
    parameters.dcharge = $('#dcharge').val();
    parameters.SEDate = $('#SEDate').val();
    parameters.salePayment = salePayment;
    parameters.posData = posData;
    parameters.fnval = fnval;
    parameters.wCustomer = wCustomer;
    parameters.roundoff = $('#round-Off').val();
    parameters.istax = $('#tax_disc').prop('checked');

    $.ajax({
        async: true,
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
                    BindBill(e);
                } else {
                    // $('.ajax_response', res_success).text(e.message);
                    // $('.AlertDiv').prepend(res_success);
                }
                setInterval(window.location.href = '/POS/Index', 120);
            } else {

                //  $('.ajax_response', res_danger).text(e.message);
                //  $('.AlertDiv').prepend(res_danger);
                $("button").prop('disabled', false); // enable button
            }
        }
    });
}
//edit
function EditFormSubmitres(fnval) {
    var url = $('#myposforms').action;
    var taxptot = 0;
    var itemcount = parseFloat($('#ItemCount').val());
    $('#POSForm .taxper').each(function () {
        taxptot += parseFloat($(this).val());
    });
    var taxper = (taxptot / itemcount).toFixed(2);
    var HTMLtbl = {
        getData: function (divid) {
            var data = [];
            divid.find('.price_main').each(function (rowIndex, r) {
                var cols = {};
                $(this).find('input,textarea,select,.price,.subtot,.descr,.inote').each(function (colIndex, c) {
                    itid = $(this).attr('data-name').split(' ')[0];
                    itval = ($(this).val() != "") ? $(this).val() : $(this).text();
                    cols[itid] = itval;
                });
                data.push(cols);
            });
            return data;
        }
    }

    var salesEntry = {
        'SENo': $('#SENo').val(),
        'BillNo': $('#BillNo').val(),
        'SEDate': $('#SEDate').val(),
        'SECashier': $('#SECashier').val(),
        'Customer': $('#ddlCustomer').val(),
        'CustomerType': $('#CustomerType').val(),
        "PayType": $('#PayMethod').val(),
        'dcharge': $("#dcharge").val(),
        'SEItems': $('#ItemCount').val(),
        'SEItemQuantity': $('#ItemQty').val(),
        'SESubTotal': $('#total').val(),
        'SETax': taxper,
        'SETaxAmount': $('#total_tax').val(),
        'SEDiscount': $('.disc-am').val(),
        'SEGrandTotal': $('#total_payable').val(),
        'SENote': $('#SENote').val(),
        'taxAFdisc': $('#tax_disc').prop('checked'),
        'OrderRefer': $('#OrderId').val(),
    }

    var posData = {
        'PayMethod': $('#PayMethod').val(),
        'TotTender': $('#total_paying').text(),
        'ChangeDue': $('#balance').text(),
        'PayMode': $('#PayMode option:selected').text(),
    }
    var wCustomer = {
        'CustomerName': $('#CustomerName').val(),
        'MobileNo': $('#MobileNo').val()
    }
    var salePayment = {
        'SEPaidAmount': $('#amount').val()
    }
    var data = HTMLtbl.getData($('#POSForm'));
    var parameters = {};
    parameters.saledata = salesEntry;
    parameters.seItems = data;
    parameters.SEDate = $('#SEDate').val();
    parameters.salePayment = salePayment;
    parameters.posData = posData;
    parameters.fnval = fnval;
    parameters.wCustomer = wCustomer;
    parameters.dcharge = $('#dcharge').val();
    parameters.roundoff = $('#round-Off').val();
    parameters.istax = $('#tax_disc').prop('checked');

    $.ajax({
        async: true,
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
                    BindBill(e);
                } else {
                    // $('.ajax_response', res_success).text(e.message);
                    //  $('.AlertDiv').prepend(res_success);
                }
                window.location.href = '/POSRES/Index';
            } else {

                // $('.ajax_response', res_danger).text(e.message);
                //  $('.AlertDiv').prepend(res_danger);
                $("button").prop('disabled', false); // enable button
            }
        }
    });
}
// bill binding function
function BindBill(data) {
    $("#lbltaxamout").text(data.sales.SETaxAmount);
    $("#lblexctax").text(parseFloat(data.sales.SEGrandTotal-data.sales.SETaxAmount).toFixed(2));
    $("#lblBillNo").text(data.sales.BillNo);
    $("#lblPONo").text(data.sales.PONo);
    if (data.sales.totalpayed != null) {
        $("#lbltotalpaying").text((data.sales.totalpayed.tendering).toFixed(2));
        $("#lblbalance").text((data.sales.totalpayed.tendering - (data.sales.SEGrandTotal).toFixed(2)).toFixed(2));
        $("#lbldelivery2").text(data.sales.oc.dcharge.toFixed(2));
    }
    else {
        $("#lblbalance").text("0.00");
    }
    $("#lbltotals").text((data.sales.SEGrandTotal).toFixed(2));
    if (data.sales.ordertype != null) {
        if (data.sales.ordertype.OrderType == 0)
            $("#lblBillOrderTypein").text(" Take Away");
        else if (data.sales.ordertype.OrderType == 1)
            $("#lblBillOrderTypein").text(" Delivery");
        else
            $("#lblBillOrderTypein").text(" Table: " + data.sales.ordertype.Table);
    }
    var custDetails = data.sales.CustomerName;
   

 
    var crtime = "";
    if (data.sales.createtime != null)
        {
        crtime = convertToTime(data.sales.createtime.CreatedDate);
    }
    $("#lblDate").text(convertToDate(data.sales.Date) + " " +crtime );
    if (convertToDate(data.sales.Date) == "01-01-1970")
        $("#lblDate").text(data.sales.Date);
    $("#lblEmployee").text(data.sales.Cashier);
    $("#lblCustomer").html(custDetails);

    var itemspint = data.item;
    $('.service').remove();
    var itemsData = bindItem(itemspint);
    $('#itemtable').append(itemsData);
    $("#lblTC").text(data.sales.TermsAndCondition);

    $("#lblTotal").text((data.sales.SETotal - data.sales.SETaxAmount).toFixed(2));
    $("#lblTax").text((data.sales.SETaxAmount).toFixed(2));
    $("#lblGTotal").text((data.sales.SEGrandTotal).toFixed(2));

    $("#lblpay").text(data.PosDate.PayMethod);
    $("#lbltender").text((data.PosDate.TotTender));
    $("#lbldue").text((data.PosDate.ChangeDue));
    if (data.sales.SEDiscount != 0) {
        $("#lblDiscount").text((data.sales.SEDiscount).toFixed(2));
    }
    else {
        $(".discpt").hide();
    }
    $("#lblDiscount").text((data.sales.SEDiscount).toFixed(2));
    if (data.billsundry != null) {
        $("#lblRoundOff").text((data.billsundry.BsAmount).toFixed(2));
    } else {
        $(".roundpt").hide();
    }


    var temp = $('body').html();
    var originalpage = document.body.innerHTML;
    //var printContent = $('#printit').html();
    //$('body').html(printContent);
    //$('body').css('padding-right', '0px');
    //$('title').html(data.sales.BillNo);
    //window.print();


    printDiv();
   // $('body').html(temp);
   
    //resetfiledres();
    //$("#dinein").text("Dinein");
    //$("#delivery").text("Delivery");
 
    $(".button").prop("disabled", false);
    
    //window.location = "/POSRES/Create";

}

function bindItem(pitem) {

    var str = "";
    var count = 1;
    $.each(pitem, function (i, item) {
        var old = (item.olditem === undefined) ? 0 : (item.olditem != null) ? item.olditem : 0;
        var quantity = (item.olditem === undefined) ? item.ItemQuantity : (item.ItemQuantity - old);
        if (quantity > 0) {
            var itemname = "";
            if (item.ItemArabic != null) {
                itemname = item.ItemName + " " + item.ItemArabic;
            }
            else {
                if (item.realname != null)
                    itemname = item.ItemName + " " + item.realname.ItemSizeName;
                else
                    itemname = item.ItemName
            }

            if (item.type == "Bundle") {
                var desc = "<br/>[<span class='descr' data-name='Note'>";
                $.each(item.bundleitem, function (j, itemss) {
                    desc += itemss.ItemCode + " - " + itemss.ItemName;
                    desc += " - " + (itemss.quantity).toFixed(2) + " ";
                    desc += (itemss.ItemUnitName != null) ? itemss.ItemUnitName : "";
                    desc += "<br/>";
                });
                desc += "</span>]";
                itemname = itemname + desc;
            }
            if (item.ItemNote != "" && item.ItemNote != null && item.ItemNote != "undefined") {
                //itemname = itemname + "<br />" + item.ItemNote;
            }
            var unitname = item.ItemUnitName != null ? item.ItemUnitName : "";
            var totalamount = parseFloat(item.ItemSubTotal) + parseFloat(item.ItemTaxAmount);
            str += '<tr class="service">';
            //   str += '<td>' + count + '</td>';
            str += ' <td class="item"><p class="itemtext">' + itemname + '</p></td>';
            /*str += ' <td class="qty"><p class="itemtext">' + unitname + '</p></td>';*/
            str += ' <td class="qty"><p class="itemtext">' + (quantity).toFixed(2) + '</p></td>';
            str += ' <td class="qty"><p class="itemtext">' + (item.ItemUnitPrice).toFixed(2) + '</p></td>';
        //    str += ' <td class="qty"><p class="itemtext">' + (item.ItemTaxAmount).toFixed(2) + '</p></td>';
            str += ' <td class="rate"><p class="itemtext">' + (totalamount).toFixed(2) + '</p></td>';
            str += '</tr>';
            count++;
        }
    });
    return str;
}

// quick cash
var pi = 'amount', pa = 2;
var currency=[];
$(document).on('click', '.quick-cash', function () {
    if ($('#quick-payable').find('span.badge').length) {
        $('#clear-cash-notes').click();
    }
  /* var note= $.each($(".quick-cash"), function (i, item) {
       var notename = $(item).contents().filter(function () {
           return this.nodeType == 3;
       }).text();
       currency[notename] = $(item).contents('span').filter(function () {
           return this.nodeType == 3;
       }).text();
       
    });*/
    var $quick_cash = $(this);
    var amt = $quick_cash.contents().filter(function () {
        return this.nodeType == 3;
    }).text();
    /*currency[amt]=$quick_cash.contents('span').filter(function () {
        return this.nodeType;
    }).text();*/
    var th = ',';
    var $pi = $('#' + pi);
    amt = parseFloat(amt.split(th).join("")) * 1 + $pi.val() * 1;
    $pi.val(parseFloat(amt)).focus();
    var note_count = $quick_cash.find('span');
    if (note_count.length == 0) {
        $quick_cash.append('<span class="badge">1</span>');
    } else {
        note_count.text(parseInt(note_count.text()) + 1);
    }
    cashBalance();
});

$(document).on('click', '#clear-cash-notes', function () {
    $('.quick-cash').find('.badge').remove();
    $('#' + pi).val('0').focus();
    cashBalance();
});
/* pos reference 
https://spos.tecdiary.com/pos
http://www.relypos.com/
https://codecanyon.net/item/zar-pos-point-of-sale-web-application/15955540
*/



function printBill(data) {
    $("#lblBillNo").text(data.sales.BillNo);
    $("#lblBillOrderNo").text(data.sales.OrderNo);
    $("#lblBillCustomer").text(data.sales.CustName);
    $("#lblBillDate").text(convertToDate(data.sales.Date));
    $("#lblBillTotal").text((data.sales.SubTotal).toFixed(2));
    $("#lblBillTax").text(data.sales.SETaxAmount);
    $("#lblBillGTotal").text(data.sales.SEGrandTotal);

    $("#lblBillTC").text(data.sales.TermsAndCondition);

    $("#lblBillDiscAmt").text((data.sales.SEDiscount).toFixed(2));
    if (data.billsundry != null) {
        $("#lblBillRoundOff").text((data.billsundry.BsAmount).toFixed(2));
    } else {
        $("#lblBillRoundOff").text("0.00");
    }
    $("#lblBillpay").text(data.sales.PayType);

    $('.service').remove();
    var itemsData = bindItem(data.item);
    $('#billitemtable').append(itemsData);

    //var originalpage = document.body.innerHTML;
    var printContent = $('#printBill').html();
    $('body').html(printContent);
    $('body').css('padding-right', '0px');
    $('title').html(data.sales.BillNo);
    window.print();

}
function printBillRES(data) {
    $("#lblBillNo").text(data.sales.BillNo);
    $("#lblBillOrderNo").text(data.sales.OrderNo);
    $("#lbldelivery").text((data.sales.dcharge).toFixed(2));
    $("#lbltotalpaying").text(data.sales.totalpayed)


    var custDetails = data.sales.CustomerName;
    if (data.sales.customer.Location != "" && data.sales.customer.Location != null) {
        custDetails += "<br/><b>Phone :</b> " + data.sales.customer.Location;
    }
    if (data.sales.Mobile != "" && data.sales.Mobile != null) {
        custDetails += "<br/> <b>Mobile No :</b> " + data.sales.Mobile;
    }
    if (data.sales.customer.Addres != "" && data.sales.customer.Addres != null) {
        custDetails += "<br/> <b>Address :</b><br/>" + data.sales.customer.Addres;
    }
    $("#lblBillCustomer").html(custDetails);

    //$("#lblBillCustomer").text(data.sales.CustName);
    $("#lblBillDate").text(convertToDate(data.sales.Date));
    $("#lblBillTotal").text((data.sales.SubTotal).toFixed(2));
    $("#lblBillTax").text(data.sales.SETaxAmount);
    $("#lblBillGTotal").text((data.sales.SEGrandTotal).toFixed(2));

    $("#lblBillTC").text(data.sales.TermsAndCondition);

    if (data.sales.SEDiscount > 0) {
        $("#trbilldiscount").show();
        $("#lblBillDiscount").text((data.sales.SEDiscount).toFixed(2));
    } else {
        $("#trbilldiscount").hide();
    }


    if (data.sales.Table != null)
        $("#lblBillOrderType").text(" Table: " + data.sales.Table);
    else if (data.sales.OrderType == 0)
        $("#lblBillOrderType").text(" Take Away");
    else if (data.sales.OrderType == 1)
        $("#lblBillOrderType").text(" Delivery");

    $('.service').remove();
    var itemsData = bindItem(data.item);
    $('#billitemtable').append(itemsData);

    var originalpage = document.body.innerHTML;
    $('title').html(data.sales.BillNo);
    var printContent = $('#printBills').html();
    var temp = $('body').html();
   // $('body').html(printContent);
    //$('body').css('padding-right', '0px');

   // window.print();
    //$('body').html(loadeddata);

    //$('title').html(data.sales.BillNo);
    //$("#printBills").removeClass("hidden");
    //$("#printBills").printThis({
    //    debug: false,
    //});
    //setTimeout(function () { window.location.href = '/POS/Create'; }, 1000);
    // window.location.href = '/POS/Create';
    printDiv2();
  //printDiv
   // generatePDF();
   // $(".modal-backdrop").hide();
}
function printDiv() {
    var divToPrint = document.getElementById('printit');
    var newWin = window.open('', 'Print-Window');
    newWin.document.open();
    newWin.document.write('<html><body onload="window.print()">' + divToPrint.innerHTML + '</body></html>');
    newWin.document.close();
    setTimeout(function () {
         newWin.close();


    }, 1000);
}

function printDiv2() {
    var divToPrint = document.getElementById('printBills');
    var newWin = window.open('', 'Print-Window');
    newWin.document.open();
    newWin.document.write('<html><body onload="window.print()">' + divToPrint.innerHTML + '</body></html>');
    newWin.document.close();
    setTimeout(function () {

       newWin.close();
        

    }, 1000);
}
//kot orders
function orderkot() {
    $.ajax({
        async: true,
        cache: false,
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/Order/KotOrders",
        data: { id: '' },
        success: function (item) {
            //if (item != "") {
            $('#orderlistkot').html("");
            $('#orderlistkot').append(allkotlist(item));
            try {
                filterData.del('#kotportfoliolist');
            } catch (x) { }
            filterData.init('#kotportfoliolist', '.pfkot', '.fltrkot');
            //} else {
            //    $('#running-orders').modal("hide");
            //}
        }
    });
}
$('#txtfilterrunning').keyup(function () {
    var val = $(this).val().toString().toUpperCase();
    var state = $("#kotportfoliolist").mixItUp('getState');
    var $filtered = state.$targets.filter(function (index, element) {
        return $(this).text().toString().indexOf(val.trim()) >= 0;
    });

    $("#kotportfoliolist").mixItUp('filter', $filtered);
});
function allkotlist(data) {
    if (data != "") {
        var datas = "";
        var Menu = '<div id="tblscrol"><ul id="filterskot" class="filter-head">';
        var topMenu = "";
        var typeMenu = "";
        var proditems = '<div id="kotportfoliolist" class="filter-items">';
        var type = "";
        var area = "";
        var myclass = "";
        var areaclass = "";
        var table = "";
        var deletebtn = "";
        $.each(data, function (i, item) {
            count = i + 1;
            prod = '';


            if (item.AreaId != area && item.OrderType == 2 && item.AreaId == null) {
                area = null;
                myclass = 'area_' + 0;
                topMenu += '<li><span class="fltrkot" data-filter=".' + myclass + '">Dine In</span></li>';
                table = '<p>Table</p>';
            }
            if (item.AreaId != area && item.OrderType == 2 && item.AreaId != null) {
                area = item.AreaId;
                myclass = 'area_' + item.AreaId;
                topMenu += '<li><span class="fltrkot" data-filter=".' + myclass + '">Dine In</span></li>';
            }
            if (item.OrderType == 2 && item.AreaId != null) {
                table = '<div>Table: ' + item.TableName + '</div>';
            }
            if (item.OrderType != parseInt(type) && item.OrderType != 2) {
                type = item.OrderType;
                myclass = 'type_' + item.OrderType;
                if (type == 0) {
                    typeMenu += '<li><span class="fltrkot" data-filter=".' + myclass + '">Take Away</span></li>';
                }
                if (type == 1) {
                    typeMenu += '<li><span class="fltrkot" data-filter=".' + myclass + '">Delivery</span></li>';
                }
            }
            if (item.DelPer) {
                deletebtn = '<button type="button" data-id="' + item.POSOrderId + '" class="deleteorder  btn btn-danger btn-half">Delete</button> ';
            } else {
                deletebtn = "";
            }

            var date = convertToDate(item.OrderDate);
            var custname = item.CustName != "" ? item.CustName : "";
            prod = '<div class="pfkot filter-item ' + myclass + '" data-cat="' + myclass + '" id="kot_' + item.POSOrderId + '" data-id="' + item.POSOrderId + '">' +
                '<div class="pfkot-wrapper">' +
                '<div class="gallery">' +
                '<div class="images"></div>' +
                //'<img src="/uploads/itemimages/' + itemimage + '"  class="images">' +
                '<div class="top">' +
                '<div class="text-note"><h5>Order No : ' + item.OrderNo + '</h5>' +
                '' + date + "<br/>" + table + 'No of Item : ' + item.ItemCount + '' +
                '<br/>Cus.Name : ' + custname +
                '<br/>Employee : ' + item.employee +
                '<p>' +
                '<button type="button" data-id="' + item.POSOrderId + '" class="selectorder  btn btn-success btn-half">Select</button>' + deletebtn + ' ' +
                '</p>';
            var orderitemname = "";
            $.each(item.orderitems, function (i, orderitems) {
                orderitemname = orderitemname + "<div style='font-size:10px !important'>" + orderitems.ItemName + "</div>";
            });
            prod = prod + orderitemname;
            prod = prod +
                '</div>' +
                '</div>' +
                '</div>' +
                '</div>' +
                '</div>';
            proditems += prod;

        })

        allclass = '<li><span class="fltrkot active" data-filter="all">All</span></li>';

        Menu += allclass + " " + typeMenu + " " + topMenu + "</ul></div>";
        proditems += "</div>";
        datas = Menu + proditems;
        return datas;
    } else {
        $('#orderlistkot').append("<h3 class='text-red'>Running Order Not Found !!</h3>");
    }
}

function fillorder3(dataid, status) {
    if (dataid != null) {
        $.ajax({
            url: '/Order/getOrderById',
            type: "POST",
            dataType: "JSON",
            data: { orderId: dataid },
            success: function (data) {
                //fill other details
                $('#OrderNo').val(data.OrderNo);
                $('#SEDate').val(convertToDate(data.OrderDate));
                $('#CustomerType').val(data.CustomerType);
                $('#HideOrderId').val(data.Id);
                $('#discount').val(data.Discount);

                $('#lbltableval').text(data.TableId);
                $('#lblpeoplcount').val(data.PeopleCount);


                $('#disc_m').val((data.Discount).toFixed(2));
                if (data.taxAFdisc == true) {
                    $('#tax_disc').prop('checked', true);
                } else {
                    $('#tax_disc').prop('checked', false);
                }

                //data.CustomerType == 0
                if (1 == 1) {
                    var Option = "<option selected='selected' value='" + data.Customer + "'>" + data.CustName + "</option>";
                    $("#ddlCustomer").append(Option);
                    $('#ddlCustomer').val(data.Customer).trigger('change');
                } else {
                    $('#CustomerName').val(data.CustName);
                    $('#MobileNo').val(data.Mobile);
                }
                //var woption = "<option selected='selected' value='" + data.WaiterId + "'>" + data.WaiterName + "</option>";
                //$("#ddlSalesPerson").append(woption);
                //$('#ddlSalesPerson').val(data.WaiterId).trigger('change');

                $('#HideWaiterId').val(data.WaiterId);


                $('#takeaway').removeClass("btn-primary");
                $('#delivery').removeClass("btn-primary");
                $('#dinein').removeClass("btn-primary");
                if (data.OrderType == 0) {
                    $('#takeaway').addClass("btn-primary");
                    $('#takeaway').removeClass("btn-warning");
                }
                if (data.OrderType == 1) {
                    $('#delivery').addClass("btn-primary");
                    $('#delivery').removeClass("btn-warning");
                    $("#delivery").text("Delivered : " + data.WaiterName);

                }
                if (data.OrderType == 2) {
                    $('#dinein').addClass("btn-primary");
                    $('#dinein').removeClass("btn-warning");
                    if (data.TableName != null) {
                        $("#dinein").text("Dine in on " + data.TableName);
                    }
                }

                customertype();
                $('#POSForm').html('');
                fillitems(data.Id);
            }
        });
    }
}
function deleteorder(dataid, divid) {
    if (dataid != null) {
        $.ajax({
            url: '/Order/DeleteOrderById',
            type: "POST",
            dataType: "JSON",
            data: { orderId: dataid },
            success: function (e) {
                if (e.status == true) {
                    //  $('.ajax_response', res_success).text(e.message);
                    //  $('.AlertDiv').prepend(res_success);
                    $('#' + divid + e.orderId).html('');
                } else {
                    //  $('.ajax_response', res_danger).text(e.message);
                    //   $('.AlertDiv').prepend(res_danger);
                }
            }
        });
    }
}


function fillitems(orderId) {
    $.ajax({
        url: '/Order/GetOrderItems',
        type: "POST",
        dataType: "JSON",
        data: { orderId: orderId },
        success: function (data) {
            var ln = data.item.length;
            $.each(data.item, function (i, item) {
                createRow(item, "sales", 5);
                ln--;
            });
            if (ln == 0) {
                discPerc();
            } else {
                alert("error");
            }
            itemTotal();

        }
    });
}

$('body').on('click', '.modal-close-btn', function () {
    $('#modal-create-lg').modal('hide');
    $('#modal-create-lg').removeData('bs.modal');
});
$('#modal-create-lg').on('hidden.bs.modal', function () {
    $(this).removeData('bs.modal');
});

function customertype() {
    var custtype = $('#CustomerType').val();
    if (custtype == 1) {
        //$('.creditsale').hide();
        //$('.cashsale').show();
        //$('#ddlCustomer').prop('required', false);
        $('.creditsale').show();
    }
    if (custtype == 0) {
        //$('.cashsale').hide();
        //$('.creditsale').show();
        //$('#ddlCustomer').prop('required', true);
        $('.creditsale').show();
    }
}
// calculate amount
function discAmount() {
    var subtotal = 0;
    var discamount = 0;
    var discper = parseFloat($('#disc_p').val()) || 0;
    //subtotal = parseFloat($('#total').val()) || 0;
    subtotal = parseFloat($('.subtotal').val()) || 0;

    discamount = parseFloat((subtotal * discper) / 100) || 0;
    $('#disc_m').val(discamount.toFixed(2));
    $('.disc-am').val(discamount.toFixed(2));
    $('#disc_p').val(discper.toFixed(2));
    if ($('#tax_disc').prop('checked') == true) {
        itemDisc();
    }
    else {
        $('#itdisc').val(0.00);
    }
    itemTotal();
}
// calculate percentage
function discPerc() {
    var subtotal = 0;
    var discamount = parseFloat($('#disc_m').val()) || 0;
    var discper = 0;
    //subtotal = parseFloat($('#total').val()) || 0;
    subtotal = parseFloat($('.subtotal').val()) || 0;
    discper = parseFloat((discamount * 100) / subtotal) || 0;
    $('#disc_p').val(discper.toFixed(2));
    $('.disc-am').val(discamount.toFixed(2));
    $('#disc_m').val(discamount.toFixed(2));
    if ($('#tax_disc').prop('checked') == true) {
        itemDisc();
    }
    else {
        $('#itdisc').val(0.00);
    }
    $("#discount").val(discamount);

    itemTotal();
    // var gtotoal = $("#total").val() + discamount;
    // $("#total").val(gtotoal);
}

// form hold function
function holdOrder() {
    var url = "/POS/Holdit";
    var taxptot = 0;
    var itemcount = parseFloat($('#ItemCount').val());
    $('#POSForm .taxper').each(function () {
        taxptot += parseFloat($(this).val());
    });
    var taxper = (taxptot / itemcount).toFixed(2);
    var HTMLtbl = {
        getData: function (divid) {
            var data = [];
            divid.find('.price_main').each(function (rowIndex, r) {
                var cols = {};
                $(this).find('input,textarea,select,.price,.subtot,.descr,.inote,.item_unit').each(function (colIndex, c) {
                    itid = $(this).attr('data-name').split(' ')[0];
                    itval = ($(this).val() != "") ? $(this).val() : $(this).text();
                    cols[itid] = itval;
                });
                data.push(cols);
            });
            return data;
        }
    }
    var salesEntry = {
        'SENo': $('#SENo').val(),
        'BillNo': $('#BillNo').val(),
        'SEDate': $('#SEDate').val(),
        'SECashier': $('#ddlSalesPerson').val(),
        'Customer': $('#ddlCustomer').val(),
        'CustomerType': $('#CustomerType').val(),
        "PayType": $('#PayMethod').val(),
        'SEItems': $('#ItemCount').val(),
        'SEItemQuantity': $('#ItemQty').val(),
        'SESubTotal': $('#total').val(),
        'SETax': taxper,
        'SETaxAmount': $('#total_tax').val(),
        'SEDiscount': $('.disc-am').val(),
        'SEGrandTotal': $('#total_payable').val(),
        'SENote': $('#SENote').val(),
        'taxAFdisc': $('#tax_disc').prop('checked'),
        'OrderRefer': $('#OrderId').val(),
    }

    var posData = {
        'PayMethod': $('#PayMethod').val(),
        'TotTender': $('#total_paying').text(),
        'ChangeDue': $('#balance').text(),
        'PayMode': $('#PayMode option:selected').text(),
    }
    var wCustomer = {
        'CustomerName': $('#CustomerName').val(),
        'MobileNo': $('#MobileNo').val()
    }
    var salePayment = {
        'SEPaidAmount': $('#amount').val()
    }
    var data = HTMLtbl.getData($('#POSForm'));
    var parameters = {};
    parameters.saledata = salesEntry;
    parameters.seItems = data;
    parameters.SEDate = $('#SEDate').val();
    parameters.salePayment = salePayment;
    parameters.posData = posData;
    parameters.wCustomer = wCustomer;
    parameters.roundoff = $('#round-Off').val();

    $.ajax({
        async: true,
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
                if (e.fnval == "print") {

                    BindBill(e);


                }
                //  $('.ajax_response', res_success).text(e.message);
                //  $('.AlertDiv').prepend(res_success);
                setInterval(window.location.href = '/POS/Create', 120);
            } else {

                //  $('.ajax_response', res_danger).text(e.message);
                //  $('.AlertDiv').prepend(res_danger);
                $("button").prop('disabled', false); // enable button
            }
        }
    });
}
//hold orders list
function holdorders() {
    $.ajax({
        async: true,
        cache: false,
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/POS/holdOrders",
        data: { id: '' },
        success: function (items) {
            if (items != "") {
                var proditems = '<div id="holdlists">';
                var deletebtn = "";
                $.each(items, function (i, item) {
                    count = i + 1;
                    if (item.DelPer) {
                        deletebtn = '<button type="button" data-id="' + item.POSOrderId + '" class="holddelete btn btn-danger btn-half">Delete</button> ';
                    } else {
                        deletebtn = "";
                    }
                    var prod = '<div class="col-md-4 holdorders" id="holdor_' + item.POSOrderId + '" data-id="' + item.POSOrderId + '">' +
                        '<div class="gallery">' +
                        '<h5>Order Id : ' + item.OrderNo + '</h5>' +
                        '<h6>Date : ' + convertToDate(item.OrderDate) + '</h6>' +
                        '<p><span >No of Item : ' + item.ItemCount + '</span></p>' +
                        '<p><span >Bill Amount : ' + item.NetPayable + '</span></p>' +
                        '<p class="holdbtns"><button type="button" data-id="' + item.POSOrderId + '" class="holdmakepay btn btn-success btn-half">Make Payment</button>' + deletebtn + '</p>' +
                        '</div>' +
                        '</div>';

                    proditems += prod;
                });
                proditems += '</div>'
                $('#holdlist').html("");
                $('#holdlist').append(proditems);
            } else {
                $('#holdlist').html("");
                $('#holdlist').append("<h3 class='text-red'>Hold Order Not Found !!</h3>");
            }
        }
    });
}

// bind order form hold list
function fillorder(dataid, status) {
    if (dataid != null) {
        $.ajax({
            url: '/Order/getOrderById',
            type: "POST",
            dataType: "JSON",
            data: { orderId: dataid },
            success: function (data) {
                //fill other details
                $('#OrderNo').val(data.OrderNo);
                $('#BillNo').val(data.BillNo);
                $('#SENo').val(data.SENo);
                $('#SEDate').val(convertToDate(data.OrderDate));
                $('#CustomerType').val(data.CustomerType);
                $('#ddlCustomer').val(data.Customer).trigger("change");
                $('#round-Off').val(0);
                $('#dcharge').val(data.dcharge);
                $('#OrderId').val(data.Id);
                $('#HideOrderId').val(data.Id);

                $('#discount').val(data.Discount);
                $('#lbltableval').text(data.TableId);
                $('#lblpeoplcount').val(data.PeopleCount);
                if (data.taxAFdisc == true) {
                    $('#tax_disc').prop('checked', true);
                }

                if (1 == 1) {
                    var Option = "<option selected='selected' value='" + data.Customer + "'>" + data.CustName + "</option>";
                    $("#ddlCustomer").append(Option);
                    $('#ddlCustomer').val(data.Customer).trigger('change');
                } else {
                    $('#CustomerName').val(data.CustName);
                    $('#MobileNo').val(data.Mobile);
                }

                $('#takeaway').removeClass("btn-primary");
                $('#delivery').removeClass("btn-primary");
                $('#dinein').removeClass("btn-primary");
                if (data.OrderType == 0) {
                    $('#takeaway').addClass("btn-primary");
                    $('#takeaway').removeClass("btn-warning");
                }
                if (data.OrderType == 1) {
                    $('#delivery').addClass("btn-primary");
                    $('#delivery').removeClass("btn-warning");
                    $("#delivery").text("Delivered : " + data.WaiterName);
                    $('#HideWaiterId').val(data.WaiterId);

                }
                if (data.OrderType == 2) {
                    $('#dinein').addClass("btn-primary");
                    $('#dinein').removeClass("btn-warning");
                    if (data.TableName != null) {
                        $("#dinein").text("Dine in on " + data.TableName);
                    }
                }
                customertype();
                $('#POSForm').html('');
                fillitems(data.Id, data.Discount);
            }
        });
    }
}
function fillitems(orderId, Discount) {
    $.ajax({
        url: '/POS/GetOrderItems',
        type: "POST",
        dataType: 'json',
        data: { orderId: orderId },
        cache: true,
        success: function (data) {
            var ln = data.length;
            $.each(data, function (i, item) {
                createRow(item, 1, "edit", item.ItemUnit);
                ln--;
            });
            if (ln == 0) {
                $('#disc_m').val(Discount);
                discPerc();
            }
            else {
                alert("error");
            }
        }
    });
}
function deleteorder(dataid, divid) {
    if (dataid != null) {
        $.ajax({
            url: '/Order/DeleteOrderById',
            type: "POST",
            dataType: "JSON",
            data: { orderId: dataid },
            success: function (e) {
                if (e.status == true) {
                    // $('.ajax_response', res_success).text(e.message);
                    //  $('.AlertDiv').prepend(res_success);
                    $('#' + divid + e.orderId).html('');
                } else {
                    //  $('.ajax_response', res_danger).text(e.message);
                    //  $('.AlertDiv').prepend(res_danger);
                }
            }
        });
    }
}

// onclick alert right side icon function
$(document).on('click', '.alert .close', function (event) {
    closeAlert();
});

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
;


function printKOT(data) {
    $("#lblOrderNo").text(data.sales.OrderNo);
    $("#lblKotDate").text(convertToDate(data.sales.Date));

    if (data.sales.OrderType == "2") {
        var table = "";
        if (data.sales.Table != null) {
            table = "Table: " + data.sales.Table;
        }

        $("#lblOrderType").text("Dinin" + "; " + table);
    } else if (data.sales.OrderType == "1") {
        $("#lblOrderType").text("Delivery");
    }
    else if (data.sales.OrderType == "0") {
        $("#lblOrderType").text("Take Away");
    }

    var itemsData = bindkotItem(data.printitem);
    $('#itemtablekot').html(itemsData);

    var originalpage = document.body.innerHTML;
    //var printContent = $('#printKOT').html();
    //$('body').html(printContent);
    //$('body').css('padding-right', '0px');
    //$('title').html(data.sales.BillNo);
    //window.print();
    printDiv3();

}
function printDiv3() {
    var divToPrint = document.getElementById('printKOT');
    var newWin = window.open('', 'Print-Window');
    newWin.document.open();
    newWin.document.write('<html><body onload="window.print()">' + divToPrint.innerHTML + '</body></html>');
    newWin.document.close();
    setTimeout(function () {
        newWin.close();


    }, 1000);
}
function bindkotItem(pitem) {
    var str = "";
    var count = 1;
    $.each(pitem, function (i, item) {
        var old = (item.olditem === undefined) ? 0 : (item.olditem != null) ? item.olditem : 0;
        var quantity = (item.olditem === undefined) ? item.ItemQuantity : (item.ItemQuantity - old);
        if (quantity > 0) {
            var itemname = "";
            if (item.ItemArabic != null) {
                itemname = item.ItemName + " " + item.ItemArabic;
            } else {
                itemname = item.ItemName
            }
            if (item.Note != null && item.Note != "undefined") {
                itemname = itemname + "<br />" + item.Note;
            }
            if (item.type == "Bundle" && item.BundleType == "ComboItem") {
                var desc = "<br/>[<span class='descr' data-name='itemNote'>";
                $.each(item.bundle, function (j, itemss) {
                    desc += itemss.ItemCode + " - " + itemss.ItemName;
                    desc += " - " + (itemss.quantity).toFixed(2) + " ";
                    desc += (itemss.ItemUnitName != null) ? itemss.ItemUnitName : "";
                    desc += "<br/>";
                });
                desc += "</span>]";
                itemname = itemname + desc;
            }
            var unitname = item.ItemUnitName != null ? item.ItemUnitName : "";
            str += '<tr class="service">';
            //   str += '<td>' + count + '</td>';
            if (item.realname != null)
                itemname = item.ItemName + " " + item.realname.ItemSizeName;

            str += ' <td class="item"><p class="itemtext">' + itemname + '</p></td>';
            str += ' <td class="qty"><p class="itemtext">' + unitname + '</p></td>';
            str += ' <td class="qty"><p class="itemtext">' + (quantity).toFixed(2) + '</p></td>';
            //str += ' <td class="qty"><p class="itemtext">' + (item.ItemUnitPrice).toFixed(2) + '</p></td>';
            //str += ' <td class="rate"><p class="itemtext">' + (item.ItemSubTotal).toFixed(2) + '</p></td>';
            str += '</tr>';
            count++;
        }
    });
    return str;
}

