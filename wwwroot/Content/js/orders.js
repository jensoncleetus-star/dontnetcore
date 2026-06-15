//hold orders list
function holdorders() {
    $.ajax({
        async: true,
        cache: false,
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/Order/holdOrders",
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
                    waiter = (item.waiter != null) ? item.waiter : "";
                    var prod = '<div class="col-md-4 holdorders" id="holdor_' + item.POSOrderId + '" data-id="' + item.POSOrderId + '">' +
                                    '<div class="gallery">' +
                                       // '<h5>Order Id : ' + item.OrderNo + '</h5>' +
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
                $('#holdlist').append("<h3 class='text-red'>Hold Order Not Found !!</h3>");
            }
        }
    });
}

function checkForm(){
    var id = $(".btn-primary").attr('id');
    var result = true;

  
    
   
    if (id == null) {
        result = false;
        $('.alert ul').html("");
        $('.alert ul').append('<li>Please Select Order Type.</li>');
        $('.alert').addClass("validation-summary-errors");
        $('.alert').removeClass("validation-summary-valid");
        $('.alert').removeClass("alert-message");
        alertUpdate();
    } else {
        var waiter = $("#HideWaiterId").val();
        if (id == "delivery" && waiter == "") {
            //alert("Please Select Waiter..");
            result = true;
        }
        //if (!$("#myposforms").valid()) {
        //    result = false;
        //}        
    }
    return result;
}
function payCheck() {
    return true;
    var result = true;
    result = checkForm();
    if ($('#CustomerType').val() == 1) {
        var amount = parseFloat($('#amount').val())||0;
        var total = parseFloat($('#total_payables').text());
        if (amount < total) {
            result = false;
            $('.alert ul').html("");
            $('.alert ul').append('<li>Please Enter Valid Amount.</li>');
            $('.alert').addClass("validation-summary-errors");
            $('.alert').removeClass("validation-summary-valid");
            $('.alert').removeClass("alert-message");
            alertUpdate();
        }
    }
    return result;
}
var filterSettle = {
    init: function (e, t, f) {
        // MixItUp plugin
        // http://mixitup.io
        $(e).mixItUp({
            selectors: {
                target: t,
                filter: f
            },
            callbacks: {
                onMixEnd: function () {
                    $(".pfdel").each(function () {
                        deliverySum();
                    });
                }
            }
        });
    },
    del: function (e) {
        $(e).mixItUp('destroy');
    }
};
// delivery settle list
function deliveryList() {
    $.ajax({
        async: true,
        cache: false,
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/Order/DeliveryList",
        data: { },
        success: function (item) {
            $('#deliverylist').html("");
            $('#deliverylist').append(delItems(item));
            try {
                filterSettle.del('#delportfoliolist');
            } catch (x) { }
            filterSettle.init('#delportfoliolist', '.pfdel', '.fltrdel');
        }
    });
}
// delivery items
function delItems(data) {
    if (data != "") {
        var datas = "";
        var Menu = '<h4>Total Amount: <span id="del_tot">0.00</span></h4><div id="tbldel"><ul id="filterDel" class="filter-head">';
        var topMenu = "";
        var typeMenu = "";
        var proditems = '<div id="delportfoliolist" class="filter-items">';
        var type = "";
        var wait = "";
        var myclass = "";
        var areaclass = "";
        var table = "";
        var deletebtn = "";
        var amount = "";
        var peramount = "";
        $.each(data, function (i, item) {
            count = i + 1;
            prod = '';
            if (item.waiterid != wait) {
                myclass = 'del_' + item.waiterid;
                topMenu += '<li><span class="fltrdel" data-filter=".' + myclass + '">' + item.waiter + '</span></li>';
                wait = item.waiterid;
            }
            if (item.DelPer) {
                deletebtn = '<button type="button" data-id="' + item.POSOrderId + '" class="delvdelete btn btn-danger btn-half">Delete</button> ';
            } else {
                deletebtn = "";
            }

            prod = '<div class="pfdel filter-item ' + myclass + '" data-cat="' + myclass + '" id="deset_' + item.POSOrderId + '" data-id="' + item.POSOrderId + '">' +
                    '<div class="pfkot-wrapper">' +
                        '<div class="gallery">' +
                        '<div class="images"></div>' +
                            '<div class="top">' +
                                '<div class="text-note"><h5>Order No : ' + item.OrderNo + '</h5>' +
                                    '' + table + 'No of Item : ' + item.ItemCount + '' +
                                    '<p>Amount : <span class="del_amount">' + item.NetPayable + '</span></p>' +
                                    '<p>' +
                                        '<button type="button" data-id="' + item.POSOrderId + '" class="delvselect btn btn-success btn-half">Select</button>' + deletebtn + ' ' +
                                    '</p>' +
                                '</div>' +
                            '</div>' +
                        '</div>' +
                    '</div>' +
                '</div>';
            proditems += prod;


        });
        //amount = '<h4>Total Amount : ' + peramount + '</h4>';
        allclass = '<li><span class="fltrdel active" data-filter="all">All</span></li>';
        Menu += allclass + " " + typeMenu + " " + topMenu + "</ul></div>";
        proditems += "</div>";
        datas = Menu + proditems;
        return datas;
    } else {
        $('#deliverylist').append("<h3 class='text-red'>Delivery Settlement Not Found !!</h3>");
    }
}
function orderSubmitRES(Status, subtypes) {
    var fnval = $(".btn-primary").attr('id');
    var url = $('#myposforms')[0].action;
    var taxptot = 0;
    var itemcount = parseFloat($('#ItemCount').val());
    var orderId = $('#HideOrderId').val();
    if (subtypes == "print") {
        
    }
    else{
        var newWin = window.open('', 'Print-Window');
        newWin.document.open();
        newWin.document.write('<html><body onload="window.print()"></body></html>');
        newWin.document.close();
        setTimeout(function () {
             newWin.close();
    
    
        }, 0); 
    }
    $('#POSForm .taxper').each(function () {
        taxptot += parseFloat($(this).val());
    });
    var taxper = (taxptot / itemcount).toFixed(2);
    var HTMLtbl = {
        getData: function (divid) {
            var data = [];
            divid.find('.price_main').each(function (rowIndex, r) {
                var cols = {};
                $(this).find('input,textarea,select,.price,.subtot').each(function (colIndex, c) {
                    itid = $(this).attr('data-name').split(' ')[0];
                    itval = ($(this).val() != "") ? $(this).val() : $(this).text();
                    cols[itid] = itval;

                    //cols.push({ name: itid, index: itval });
                });
                data.push(cols);
            });
            return data;
        }
    }
    var posOrder = {
        'POSOrderId': orderId,
        'OrderNo': $('#OrderNo').val(),
        'OrderDate': $('#SEDate').val(),
        'WaiterId': $('#HideWaiterId').val(),
        'Customer': $('#ddlCustomer').val(),
        'CustomerType': $('#CustomerType').val(),

        'ItemCount': $('#ItemCount').val(),
        'Quantity': $('#ItemQty').val(),
        'SubTotal': $('#total').val(),
        'dcharge': $("#dcharge").val(),
        'Tax': taxper,
        'TaxAmount': $('#total_tax').val(),
        'Discount': $('#SEDiscount').val(),
        'NetPayable': $('#total_payable').val(),
        'OrderNote': $('#SENote').val(),
        

        'TableId': $('#lbltableval').text(),
        'PeopleCount': $('#lblpeoplcount').val()
    }
    var wCustomer = {
        'CustomerName': $('#CustomerName').val(),
        'MobileNo': $('#MobileNo').val()
    }
    var data = HTMLtbl.getData($('#POSForm'));
    var parameters = {};
    var posData = {};
    var salePayment = {};
    var subtype = "";
    if (Status == 4) {
        posData = {
            'PayMethod': $('#PayMethod').val(),
            'TotTender': $('#total_paying').text(),
            'ChangeDue': $('#balance').text(),
        }
        salePayment = {
            'SEPaidAmount': $('#amount').val()
        }
        subtype = subtypes;

    }
    var url = "";
    if (orderId != "") {
        url = "/OrderRES/Update";
    } else {
        url = "/OrderRES/Create";
    }
    parameters.OrderDate = $('#SEDate').val();
    parameters.OrderStatus = Status;
    parameters.orderitem = data;
    parameters.orderdata = posOrder;
    parameters.wCustomer = wCustomer;
    parameters.OrderType = fnval;
    parameters.fnval = subtype;
    parameters.salePayment = salePayment;
    parameters.posData = posData;
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
                
                if (Status != 4) {
                    if (e.sales.OrderStatus == "0") {
                        printKOT(e);
                    }
                    if (e.sales.OrderStatus == "1") {
                        printBillRES(e);
                    }
                   // printBillRES(e);
                }
                else {
                    var printtwo = 0;
                    if ($("#chkdual").prop('checked') == true)
                        printtwo = 1;
                    if (e.fnval == "print" || e.fnval == "print_order") {
                        BindBill(e);
                        
                        if (printtwo==1)
                        BindBill(e);
                    }

                }
                
                $("button").prop('disabled', false);
                resetfiledres();
                printtwo = 1;
                if (e.fnval == "print" || e.fnval == "print_order") {
                orderno = parseFloat(e.sales.OrderNo) + 1;
               
                $("#OrderNo").val(orderno);
            }
            else{
              
            }
                $("#cartModal").modal("hide");
                $("#payModal").modal("hide");

                //window.location = '/POSRES/Create';
                //$('.ajax_response', res_success).text(e.message);
                //$('.AlertDiv').prepend(res_success);
                //var orderno = parseInt($("#OrderNo").val()) + 1;
                //resetfiledres();
                //$("#dinein").text("Dinein");
                //$("#delivery").text("Delivery");

                //$("#OrderNo").val(orderno);
                //$("button").prop('disabled', false);
                //$(".modal-backdrop").hide()
                //$(".button").prop("disabled", false);
           
                
            } else {

              
                resetfiledres();
                $("#dinein").text("Dinein");
                $("#delivery").text("Delivery");
              
                $("button").prop('disabled', false);
                $(".modal-backdrop").hide()
                $(".button").prop("disabled", false);
            }
            $('#SEDiscount').val(0);
        }
    });

}
function generatePDF() {
    // Choose the element that your content will be rendered to.
    const element = document.getElementById('invoice');
    // Choose the element and save the PDF for your user.
    html2pdf().from(element).save();
}
function orderSubmitRESEdit(Status, fnval) {
    var url = $('#myposforms')[0].action;
    var taxptot = 0;
    var itemcount = parseFloat($('#ItemCount').val());
    $('#POSForm .taxper').each(function () {
        taxptot += parseFloat($(this).val());
    });
    var SaleEntryID = getQueryString('');
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
        'SalesEntryId': SaleEntryID,
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

function orderSubmit(Status, subtypes) {
    var fnval = $(".btn-primary").attr('id');
    var url = $('#myposforms')[0].action;
    var taxptot = 0;
    var itemcount = parseFloat($('#ItemCount').val());
    var orderId = $('#HideOrderId').val();

    $('#POSForm .taxper').each(function () {
        taxptot += parseFloat($(this).val());
    });
    var taxper = (taxptot / itemcount).toFixed(2);
    var HTMLtbl = {
        getData: function (divid) {
            var data = [];
            divid.find('.price_main').each(function (rowIndex, r) {
                var cols = {};
                $(this).find('input,textarea,select,.price,.subtot').each(function (colIndex, c) {
                    itid = $(this).attr('data-name').split(' ')[0];
                    itval = ($(this).val() != "") ? $(this).val() : $(this).text();
                    cols[itid] = itval;

                    //cols.push({ name: itid, index: itval });
                });
                data.push(cols);
            });
            return data;
        }
    }
    var posOrder = {
        'POSOrderId': orderId,
        'OrderNo': $('#OrderNo').val(),
        'OrderDate': $('#SEDate').val(),
        'WaiterId': $('#HideWaiterId').val(),
        'Customer': $('#ddlCustomer').val(),
        'CustomerType': $('#CustomerType').val(),

        'ItemCount': $('#ItemCount').val(),
        'Quantity': $('#ItemQty').val(),
        'SubTotal': $('#total').val(),
        'Tax': taxper,
        'TaxAmount': $('#total_tax').val(),
        'Discount': $('#SEDiscount').text(),
        'NetPayable': $('#total_payable').val(),
        'OrderNote': $('#SENote').val(),


        'TableId': $('#lbltableval').text(),
        'PeopleCount': $('#lblpeoplcount').val()
    }
    var wCustomer = {
        'CustomerName': $('#CustomerName').val(),
        'MobileNo': $('#MobileNo').val()
    }
    var data = HTMLtbl.getData($('#POSForm'));
    var parameters = {};
    var posData = {};
    var salePayment = {};
    var subtype = "";
    if (Status == 4) {
        posData = {
            'PayMethod': $('#PayMethod').val(),
            'TotTender': $('#total_paying').text(),
            'ChangeDue': $('#balance').text(),
        }
        salePayment = {
            'SEPaidAmount': $('#amount').val()
        }
        subtype = subtypes;
        
    }
    var url = "";
    if (orderId != "") {
        url = "/Order/Update";
    } else {
        url = "/Order/Create";
    }
    parameters.OrderDate = $('#SEDate').val();
    parameters.OrderStatus = Status;
    parameters.orderitem = data;
    parameters.orderdata = posOrder;
    parameters.wCustomer = wCustomer;
    parameters.OrderType = fnval;
    parameters.fnval = subtype;
    parameters.salePayment = salePayment;
    parameters.posData = posData;
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
                if (Status != 4) {
                    if (e.sales.orderStatus == "PrintKOT") {
                        printKOT(e);
                    }
                    if (e.sales.orderStatus == "PrintBill") {
                        printBill(e);
                    }
                }
                else {
                    if (e.fnval == "print" || e.fnval == "print_order") {
                        BindBill(e);
                    }
                    
                }
                $('.ajax_response', res_success).text(e.message);
                $('.AlertDiv').prepend(res_success);

                 window.location.href = '/POS/Create';
            } else {

                $('.ajax_response', res_danger).text(e.message);
                $('.AlertDiv').prepend(res_danger);
                $("button").prop('disabled', false); // enable button
            }
        }
    });

}

function deliverySum() {
    var total = 0;
    $(".pfdel").each(function () {
        var style = $(this).attr('style');
        if (style == 'transition: none; display: inline-block;'||style=='display: inline-block; transition: none;') {
            var amount = $(this).find('.del_amount').text();
            total += parseFloat(amount);
        }
    });
    $("#del_tot").text(total.toFixed(2));
   // return total;
}

//duplicate kot
function duplicatekot() {
    $.ajax({
        async: true,
        cache: false,
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/Order/DuplicateKotOrders",
        data: {},
        success: function (item) {
            $('#duplicatekotlist').html("");
            $('#duplicatekotlist').append(dupkotItems(item));
            try {
                filterData.del('#dkotportfoliolist');
            } catch (x) { }
            filterData.init('#dkotportfoliolist', '.pfkot', '.fltrkot');
        }
    });
}

function dupkotItems(data) {
    if (data != "") {
        var datas = "";
        var Menu = '<div id="tbldkotscrol"><ul id="filtersdkot" class="filter-head">';
        var topMenu = "";
        var typeMenu = "";
        var proditems = '<div id="dkotportfoliolist" class="filter-items">';
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
                topMenu += '<li><span class="fltrkot" data-filter=".' + myclass + '">' + item.AreaName + '</span></li>';
            }
            if (item.OrderType == 2 && item.AreaId != null) {
                table = '<p>' + item.TableName + '</p>';
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


            prod = '<div class="pfkot filter-item ' + myclass + '" data-cat="' + myclass + '" id="dkot_' + item.POSOrderId + '" data-id="' + item.POSOrderId + '">' +
                    '<div class="pfkot-wrapper">' +
                        '<div class="gallery">' +
                        '<div class="images"></div>' +
                            //'<img src="/uploads/itemimages/' + itemimage + '"  class="images">' +
                            '<div class="top">' +
                                '<div class="text-note"><h5>Order No : ' + item.OrderNo + '</h5>' +
                                    '' + table + 'No of Item : ' + item.ItemCount + '' +
                                    '<p>' +
                                        '<button type="button" data-id="' + item.POSOrderId + '" class="selectdkot  btn btn-success btn-full">Print</button> ' +
                                    '</p>' +
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
        $('#duplicatekotlist').append("<h3 class='text-red'>KOT Order Not Found !!</h3>");
    }
}
function printdupkot(dataid) {
    $.ajax({
        url: '/Order/OrdersAndItemById',
        type: "GET",
        dataType: "JSON",
        data: { orderId: dataid },
        success: function (data) {
            printKOT(data);
            window.location.href = '/POSRES/Create';
        }
    });
}

//duplicate bill
function duplicatebill() {
    var billno = $("#dupbillorder").val();
    var date = $("#dupbilldate").val();
    if (billno != "" || date != "") {
        $.ajax({
            url: "/Order/DuplicateBills",
            type: "GET",
            dataType: "JSON",
            data: { billno: billno, date: date },
            success: function (data) {
                if (data != "") {
                    $('#duplicatebilllist').html("");
                    $('#duplicatebilllist').append(dupbillItems(data));
                } else {
                    $('#duplicatebilllist').append("<h3 class='text-red'>Data Not Found !!</h3>");
                }
            }
        });
    } else {
        $('#duplicatebilllist').append("<h3 class='text-red'>Please Enter Bill No. Or Date !!</h3>");
    }
}
function dupbillItems(data) {
    var datas = "";
    var Menu = '<div id="tbldbillscrol"><ul id="filtersdbill" class="filter-head">';
    var topMenu = "";
    var typeMenu = "";
    var proditems = '<div id="dbillportfoliolist" class="filter-items">';
    var type = "";
    var area = "";
    var myclass = "";
    var areaclass = "";
    var table = "";
    var deletebtn = "";
    $.each(data, function (i, item) {
        count = i + 1;
        prod = '';
        prod = '<div class="pfkot filter-item ' + myclass + '" id="dkot_' + item.SalesEntryId + '" data-id="' + item.SalesEntryId + '" style="display:inline-block">' +
                '<div class="pfkot-wrapper">' +
                    '<div class="gallery">' +
                    '<div class="images"></div>' +
                        '<div class="top">' +
                            '<div class="text-note"><h5>Bill No : ' + item.BillNo + '</h5>' +
                             ' Grand Total : ' + item.SEGrandTotal + '' +
                                '<p>' +
                                    '<button type="button" data-id="' + item.SalesEntryId + '" class="printdbill  btn btn-success btn-full">Print</button> ' +
                                '</p>' +
                            '</div>' +
                        '</div>' +
                    '</div>' +
                '</div>' +
            '</div>';

        proditems += prod;

    })
    Menu += typeMenu + " " + topMenu + "</ul></div>";
    proditems += "</div>";
    datas = Menu + proditems;
    return datas;
}
function printdupbill(dataid) {
    $.ajax({
        url: '/Order/SalesEntryAndItemById',
        type: "GET",
        dataType: "JSON",
        data: { entryId: dataid },
        success: function (data) {
            printBill(data);
            window.location.href = '/POSRES/Create';
        }
    });
}
//waiter list
function waiterlist() {
    $.ajax({
        async: true,
        cache: false,
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/Employee/SearchEmployee",
        data: {},
        success: function (item) {
            $('#waiterlisttable').html("");
            $('#waiterlisttable').append(getwaiter(item));
        }
    });
}
function getwaiter(data) {
    if (data != "") {
        var datas = "";
        var Menu = '<div id="tblwaiterscrol"><ul id="filterswaiter" class="filter-head">';
        var topMenu = "";
        var typeMenu = "";
        var proditems = '<div id="dwaiterportfoliolist" class="filter-items">';
        var type = "";
        var area = "";
        var myclass = "";
        var areaclass = "";
        var table = "";
        var deletebtn = "";
        $.each(data, function (i, item) {
            count = i + 1;
            prod = '';
            prod = '<div class="pfkot filter-item ' + myclass + '" data-cat="' + myclass + '" id="wait_' + item.id + '" data-id="' + item.id + '" style="display:inline-block">' +
                    '<div class="pfkot-wrapper">' +
                        '<div class="gallery">' +
                        '<div class="images"></div>' +
                            '<div class="top">' +
                                '<div class="text-note"><h5>Driver</h5>' +
                                ' Name : ' + item.text + '' +
                                    '<p>' +
                                        '<button type="button" data-id="' + item.id + '" name="' + item.text + '" class="selectwaiter  btn btn-success btn-full">Select</button> ' +
                                    '</p>' +
                                '</div>' +
                            '</div>' +
                        '</div>' +
                    '</div>' +
                '</div>';

            proditems += prod;

        })
        Menu += typeMenu + " " + topMenu + "</ul></div>";
        proditems += "</div>";
        datas = Menu + proditems;
        return datas;
    } else {
        $('#waiterlisttable').append("<h3 class='text-red'>No Driver Found, Please add driver..!!</h3>");
    }
}
function paymethodchange() {
    var Paymethod = $('#PayMethod').val();
    if (Paymethod == "Credit") {
        $('#amount').val(0);
        $('#amount').attr("readonly", true);
        $("#amount").keypad('disable');
        $("#divPayMode").hide();
    }
    else if (Paymethod == "Card") {
        var total = parseFloat($('#total_payables').text());
        $('#amount').attr("readonly", true);
        $('#amount').val(total.toFixed(2));
        $("#amount").keypad('disable');
        $("#divPayMode").show();
    }
    else {
        $('#amount').attr("readonly", false);
      /*  $("#amount").keypad('enable');*/
        $("#divPayMode").hide();
    }
}