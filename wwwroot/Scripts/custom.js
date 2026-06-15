// alert constants
res_success = $('<div class="alert alert-success alert-message" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button><h4><i class="icon fa fa-check"></i>Success !</h4><p class="ajax_response"></p></div>');
res_info = $('<div class="alert alert-info alert-message" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button><h4><i class="icon fa fa-check"></i>Success !</h4><p class="ajax_response"></p></div>');
res_warning = $('<div class="alert alert-warning alert-message" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button><h4><i class="icon fa fa-warning"></i>Warning !</h4><p class="ajax_response"></p></div>');
res_danger = $('<div class="alert alert-danger alert-message" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button><h4><i class="icon fa fa-ban"></i>"Error !"</h4><p class="ajax_response"></p></div>');

// for menu bar dropdown stuck problem
$(window).scroll(function (e) {
    var scroll = $(window).scrollTop();
    if (scroll >= 0) {
        $('.treeview-menu').addClass("navbar-hide");
        $('.treeview-menu').children().find("ul").removeClass("navbar-hide");
        var i = scroll;
        $(".navbar-hide").css('margin-top', -i);
    } else {
        $(".navbar-hide").css('margin-top', 0);
        $('.treeview-menu').removeClass("navbar-hide");
    }
});
// fade alert after 5 second
function fadeAlert() {
    window.setTimeout(function () {
        $(".alert-message").alert("close");
    }, 5000);
}

// Date convert function
function convertToDate(data) {
    // The 6th+ positions contain the number of milliseconds in Universal Coordinated Time between the specified date and midnight January 1, 1970.
    var dtStart = new Date(parseInt(data.substr(6)));
    // Format using moment.js.
    var dtStartWrapper = moment(dtStart);
    return dtStartWrapper.format("DD-MM-YYYY");
}
function convertToDatecheck(data) {
    // The 6th+ positions contain the number of milliseconds in Universal Coordinated Time between the specified date and midnight January 1, 1970.
    var dtStart = new Date(parseInt(data.substr(6)));
    // Format using moment.js.
    var dtStartWrapper = moment(dtStart);
    return dtStartWrapper.format("DD-MM-YYYY");
}
function convertToMonthYear(data) {
    // The 6th+ positions contain the number of milliseconds in Universal Coordinated Time between the specified date and midnight January 1, 1970.
    var dtStart = new Date(parseInt(data.substr(6)));
    // Format using moment.js.
    var dtStartWrapper = moment(dtStart);
    return dtStartWrapper.format("MMMM-YYYY");
}
function convertToTime(data) {
    // The 6th+ positions contain the number of milliseconds in Universal Coordinated Time between the specified date and midnight January 1, 1970.
    var dtStart = new Date(parseInt(data.substr(6))).toLocaleString('en-GB', { timeZone: 'Asia/Dubai' });;
    // Format using moment.js.

    var dtStartWrapper = moment(dtStart);
    // var am_pm = dtStart.getHours() >= 12 ? "PM" : "AM";
    var time = dtStartWrapper.format("HH:mm:ss");// + " " + am_pm;
    return time;
}
function convertDateTime(data, format) {
    format = typeof format !== 'undefined' ? a : "DD-MM-YYYY hh:mm";
    var dtStart = new Date(parseInt(data.substr(6)));
    var dtStartWrapper = moment(dtStart);
    var am_pm = dtStart.getHours() >= 12 ? "PM" : "AM";
    var time = dtStartWrapper.format(format) + " " + am_pm;
    return time;
}

function convertDateTimeYMD(data, format) {
    format = typeof format !== 'undefined' ? a : "YYYY-MM-DD hh:mm";
    var dtStart = new Date(parseInt(data.substr(6)));
    var dtStartWrapper = moment(dtStart);
    var am_pm = dtStart.getHours() >= 12 ? "PM" : "AM";
    var time = dtStartWrapper.format(format) + " " + am_pm;
    return time;
}
function convertToDateYMD(data) {
    // The 6th+ positions contain the number of milliseconds in Universal Coordinated Time between the specified date and midnight January 1, 1970.
    var dtStart = new Date(parseInt(data.substr(6)));
    // Format using moment.js.
    var dtStartWrapper = moment(dtStart);
    return dtStartWrapper.format("YYYY-MM-DD");
}

function convertCommaNumber(num) {
    var amt = num.replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1,");
    return amt;
}
function modalshow(url, modalid) {
    $.ajax({
        type: "GET",
        dataType: "html",
        url: url,
        success: function (msg) {
            $(modalid + " .modal-content").html(msg);
            $(modalid).modal("show");
        }
    });
}
function createajax(url, data, modal) {
    $.ajax({
        type: "POST",
        url: url,
        data: data,
        success: function (data) {
            if (data.status) {
                $(modal).modal('hide');
                //$(modal + ' .modal-content').empty();
                $(modal + ' .modal-content').html('');
                $('.ajax_response', res_success).text(data.message);
                $('.AlertDiv').prepend(res_success);
                if (typeof oTable != 'undefined')
                    oTable.draw();
            }
            else {
                ////for (var i = 0; i < data.errors.length; i++) { 
                //    $('.ajax_response', res_danger).text(data.error[0]);
                //    $('.AlertDiv').prepend(res_danger);
                ////}

                $('.ajax_response', res_danger).text(data.message);
                $('.AlertDiv').prepend(res_danger);
            }
            fadeAlert();
        }
    })
}
function ajaxSubmit(url, data, modal) {
    $.ajax({
        type: "POST",
        url: url,
        data: data,
        success: function (data) {
            if (data.status) {
                $(modal).modal('hide');
                $('.ajax_response', res_success).text(data.message);
                $('.AlertDiv').prepend(res_success);
                if (typeof oTable != 'undefined')
                    oTable.draw(false);
            }
            else {
                $('.ajax_response', res_danger).text(data.message);
                $('.AlertDiv').prepend(res_danger);
            }
            fadeAlert();
        }
    })
}

function ajaxDelete(data) {
    if (confirm("Are you sure?")) {
        $.ajax({
            type: "POST",
            url: "/Contact/DeleteAll",
            data: { bill: data },
            success: function (data) {
                if (data == "OK") {

                    //   $('.ajax_response', res_success).text("Deleted 1 item!");
                    //  $('.AlertDiv').prepend(res_success);
                    if (typeof oTable != 'undefined')
                        oTable.draw(false);
                    window.location.reload();
                }
                else {
                    $('.ajax_response', res_danger).text(data.message);
                    $('.AlertDiv').prepend(res_danger);
                }
                fadeAlert();
            }
        })
    }
}
$('body').on('click', '.modal-close-btn', function () {
    $('#modal-delete').modal('hide');
    $('#modal-delete').removeData('bs.modal');
});
$('body').on('click', 'input', function () {
    if ($(window).width() >= 500) {
        $(this).select();
    }
});
$('.modal').on('hidden.bs.modal', function () {
    $('.modal .modal-content').empty();
    //$(this).removeData('.bs.modal');
});
$('.modal').on('hidden', function () {
    // do something…
    //$(this +' .modal-content').empty();
})
// button reset function
$("#btnreset").click(function () {
    $('form').each(function () {
        this.reset();
    });
});
// datepicker initialization
function datepickerInit() {
    $('.date').datepicker({
        format: 'dd-mm-yyyy',
        autoclose: true,
        allowInputToggle: true
    });
}
// return today date
function today() {
    var d = new Date();
    var month = d.getMonth() + 1;
    var day = d.getDate();
    var output = (('' + day).length < 2 ? '0' : '') + day + '-' +
        (('' + month).length < 2 ? '0' : '') + month + '-' +
        d.getFullYear();
    return output;
}
// enter functionality

//function focusNextElement() {
//    var focussableElements = 'a:not([disabled]), button:not([disabled]), input[type=text]:not([disabled]), [tabindex]:not([disabled]):not([tabindex="-1"])';
//    if (document.activeElement && document.activeElement.form) {
//        var focussable = Array.prototype.filter.call(document.activeElement.form.querySelectorAll(focussableElements),
//          function (element) {
//              return element.offsetWidth > 0 || element.offsetHeight > 0 || element === document.activeElement
//          });
//        var index = focussable.indexOf(document.activeElement);
//        focussable[index + 1].focus();
//    }
//}

//window.addEventListener('keydown', function (e) {
//    if (e.keyIdentifier == 'U+000A' || e.keyIdentifier == 'Enter' || e.keyCode == 13) {
//        if (e.target.nodeName === 'INPUT' && e.target.type !== 'textarea') {
//            e.preventDefault();
//            focusNextElement();
//            return false;
//        }
//    }
//}, true);

// get query string
var getQueryString = function (field, url) {
    var href = url ? url : window.location.href;
    var id = href.substring(href.lastIndexOf('/') + 1);
    return id ? id : null;
}
// search query string of format somthing.com&id=""&&type=""
var getQueryValue = function (field, url) {
    var href = url ? url : window.location.href;
    var reg = new RegExp('[?&]' + field + '=([^&#]*)', 'i');
    var string = reg.exec(href);
    return string ? string[1] : null;
};
function GetURLParameter() {
    var sPageURL = window.location.href;
    var indexOfLastSlash = sPageURL.lastIndexOf("/");

    if (indexOfLastSlash > 0 && sPageURL.length - 1 != indexOfLastSlash)
        return sPageURL.substring(indexOfLastSlash + 1);
    else
        return 0;
}
var getQueryString1 = function (field, url) {
    var href = url ? url : window.location.href;
    var reg = new RegExp('[?&]' + field + '=([^&#]*)', 'i');
    var string = reg.exec(href);
    return string ? string[1] : null;
};
function getParameterByName(name) {
    name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
    var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
        results = regex.exec(location.search);
    return results == null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
}

$("[data-hide]").on("click", function () {
    $("." + $(this).attr("data-hide")).hide();
    // -or-, see below
    // $(this).closest("." + $(this).attr("data-hide")).hide();
});

function getRoundOff(value, type) {
    var output = null;
    if (type == "floor") {
        output = Math.floor(value * 100) / 100;
    } else if (type == "ceil") {
        output = Math.ceil(value * 100) / 100;
    } else {
        output = Math.round(value * 100) / 100;
    }
    return output.toFixed(2);
}
function printSticker(id, value) {
    $("#" + id).JsBarcode(value, {
        "format": "CODE128",
        "width": 1,
        "height": 22,
        "fontSize": 7,
        "textMargin": 0,
        "marginLeft": 0,
    });
}



function printJewSticker(id, value) {
    id.JsBarcode(value, {
        "format": "CODE128",
        "width": 0.7,
        "height": 20,
        "fontSize": 16,
        "textAlign": "center",
        "textMargin": 1,
        "margin": 3,
        "textPosition": "top",
        "marginLeft": 10,
        "fontOptions": "bold"
    });
}

function alertUpdate() {
    if ($('.alert').find('.close').length == 0)
        $('.alert').prepend('<button type="button" class="close" data-hide="alert">×</button>');
}
function closeAlert() {
    $('.alert').removeClass("validation-summary-errors");
    $('.alert').addClass("validation-summary-valid");
    $('.alert').addClass("alert-message");
}


//Items Sub Menus
function ItemCategory() {
    // Item Category
    $("#ddlItemCategory").select2({
        placeholder: 'Search Item Category by Name or Code',
        minimumInputLength: 0,
        ajax: {
            url: "/ItemCategory/SearchCategory",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "default"
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

function ItemBrand() {

    $("#ddlItemBrand").select2({
        placeholder: 'Search Item Brand by Name or Code',
        minimumInputLength: 0,
        ajax: {
            url: "/ItemBrand/SearchBrand",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "default"
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
function ItemTax() {
    $("#ddlSaleTax").select2({
        placeholder: 'Search Tax',
        minimumInputLength: 0,
        ajax: {
            url: "/Tax/SearchTax",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
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

function ItemUnit() {
    $("#ItemUnitID").select2({
        placeholder: 'Search Primary Item Unit',
        minimumInputLength: 0,
        ajax: {
            url: "/ItemUnit/SearchUnit",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
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

function ItemSubUnit() {
    $("#SubUnitId").select2({
        placeholder: 'Search Secondary Item Unit',
        minimumInputLength: 0,
        ajax: {
            url: "/ItemUnit/SearchUnit",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
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
//End Item Submenus

//primary unit change
function prunitchange() {
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
}
//sec unit change
function seunitchange() {
    coFactorChange();
}



function clickstock() {
    if ($('#KeepStock').prop('checked') == true) {
        $('.StockSection').show();
    }
    else {
        $('.StockSection').hide();
    }
}

//change cofactor
function coFactorChange() {

    if ($("#ItemUnitID").prop('selectedIndex') > 0 && $("#SubUnitId").prop('selectedIndex') > 0) {
        $('#ConFactor').prop('readonly', false);

        var pry = $("#ItemUnitID").val();
        var sec = $("#SubUnitId").val();

        if (pry == sec) {
            $("#ConFactor").val(1);
            $('#ConFactor').prop('readonly', true);
        } else {
            $('#ConFactor').prop('readonly', false);
        }
    } else {
        $('#ConFactor').prop('readonly', true);
    }
}

$('#modal-container-category').on('submit', '#createform', function (e) {
    var url = $('#modal-container-category #createform')[0].action;
    var text = $("#ItemCategoryName").val();
    //alert(url)
    $('#ItemCategoryID option:selected').attr("selected", null);
    $.ajax({
        type: "POST",
        url: url,
        data: $('#modal-container-category #createform').serialize(),
        success: function (data) {
            alert(data.Id);
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
                $('.form-control[name="ItemBrandID"]').append(newOption);
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
            alert(data.Id)
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

                coFactorChange();
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

//barcode print
function bindbarcode() {
    var rows = parseInt($('#labelRows').val());
    rows = (rows > 0) ? rows : 1;
    var itemName = $('#itmname').val();
    var cfactor = $('#Cfactor').val();
    var itemPrice = $('#itmprice').val();

    if ($("#Unit").prop('selectedIndex') == 0) {
        itemPrice = itemPrice;
    } else {

        itemPrice = (itemPrice / cfactor);
    }
    var CName = $('#cname').val();
    var image = $('#cont').html();
    var table = "";
    var str = '<div>' + image + '</div><p style="margin-top: -12px;line-height: 10px;position: relative;">' + itemName + '</p>' + "<div style=font-size:15px>" + itemPrice + "</div>";
    var i = 0;
    while (i < rows) {
        var items = 1;
        table += "<tr><td> " + str + "</td></tr>";
        i++;
    }
    $('#bartable').append(table);

    var originalpage = document.body.innerHTML;
    var printContent = $('#printBarcode').html();
    $('body').html(printContent);
    $('title').html("Barcode Print");
    window.print();
}
function bindbarcodeall() {
    var table = "";
    var itcon = $("#itcontain").val();
    $.ajax({
        url: '/Item/GetItemall',
        type: "GET",
        data: { itemcontain: itcon },
        dataType: "JSON",
 
        success: function (data) {

            $.each(data, function (i, da) {
                $("#Cfactor").val(da.ConFactor);

                $("[id$=bcvalue]").val(da.Barcode);
                $("[id$=itmname]").val(da.ItemName);
                $("[id$=itmprice]").val(da.ItemCode);
                $("[id$=itemprice]").val(da.SellingPrice);
                var CName = $('#cname').val();
                printSticker('barcode', da.Barcode);
                var image = $('#cont').html();
             
                var str = '<p style="margin: 0px;position: relative;padding:0px;height:41px;">' + image + '</p><p style="margin: 0px;position: relative;">' + CName + '</p>' + "<p style=font-size:5px;line-height:5px;margin-top: -12px;position: relative;>" + da.ItemName + "</p>" + "<p style='margin-top: -12px;position: relative;' > AED : " + parseFloat(da.SellingPrice).toFixed(2) + '</p>';
                var i = 0;
                while (i < 1) {
                    var items = 1;
                    table += "<tr><td> " + str + "</td></tr>";
                    i++;
                }

            });

            $('#bartable').append(table);

            var originalpage = document.body.innerHTML;
            var printContent = $('#printBarcode').html();
            $('body').html(printContent);
            $('title').html("Barcode Print");
            window.print();
        }
    });

}
function GetCurrencyCode() {
    // Item Category
    $("#ddlCurrency").select2({
        placeholder: 'Search Currency by Name or Code',
        minimumInputLength: 0,
        ajax: {
            url: "/CurrencyMaster/SearchCurrency",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
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

    var CId = $('#ddlCurrency').val();
    if (CId != null) {
        $.ajax({
            url: '/CurrencyMaster/GetCurrencyById',
            type: "GET",
            dataType: "JSON",
            data: { CId: CId },
            success: function (result) {
                if (result != null) {
                    $('#ConRate').val(result.ConvertionRate);
                }
            }
        })
    }

    $('#ddlCurrency').on('change', function (e) {

        var CId = $('#ddlCurrency').val();
        if (CId != null) {
            $.ajax({
                url: '/CurrencyMaster/GetCurrencyById',
                type: "GET",
                dataType: "JSON",
                data: { CId: CId },
                success: function (result) {
                    if (result != null) {
                        $('#ConRate').val(result.ConvertionRate);
                    }
                }
            })
        }

    });

}

function GetBranch() {
    $("#ddlBranch").select2({
        placeholder: 'Search Branch by Name or Code',
        minimumInputLength: 0,
        ajax: {
            url: "/Branch/SearchBranch",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
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

function PrefixChange() {
    var prefix = $('#ddlprefix').val();
    if (prefix != null) {
        $.ajax({
            url: '/PrefixMaster/GetPrefixCode',
            type: 'POST',
            dataType: "JSON",
            data: { prefix: prefix },
            success: function (result) {
                $("#ItemCode").val(result.pre);
                $("#Barcode").val(result.pre);
            }
        });
    }
};


function Itemtype() {
    var type = ($("#ItemType").val());
    if (type == 2) {
        $('#Diamond').show();
        $('#Watch').hide();
        $('#commonfield').show();

        $('.normal').hide()
        $('#KeepStock').prop('checked', true);
        $('.StockSection').show();
    }
    else if (type == 3) {
        $('#Diamond').hide();
        $('#Watch').show();
        $('#commonfield').show();

        $('.normal').hide()
        $('#KeepStock').prop('checked', true);
        $('.StockSection').show();
    }
    else {
        $('#Diamond').hide();
        $('#Watch').hide();
        $('#commonfield').hide();

        $('.normal').show()
    }
}

function currency() {
    var currency = $('#ddlCurrency').val();
    $.ajax({
        url: '/Item/Convertionrate',
        type: 'POST',
        dataType: "JSON",
        data: { currency: currency },
        success: function (result) {
            $("#ConRate").val(result.conv);
        }
    });
};


function CallMC() {
    if ($("#ddlMC") != null) {
        // Material Center
        $("#ddlMC").select2({
            placeholder: 'Search Material Center by Name or Code',
            minimumInputLength: 0,
            ajax: {
                url: "/MC/SearchMC",
                dataType: 'json',
                delay: 50,
                data: function (params) {
                    return {
                        q: params.term,
                        X: "All",
                        page: params.page,
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
}

function CallMCUser() {
    if ($("#ddlMC") != null) {
        // Material Center
        $("#ddlMC").select2({
            placeholder: 'Search Material Center by Name or Code',
            minimumInputLength: 0,
            ajax: {
                url: "/MC/SearchMCUser",
                dataType: 'json',
                delay: 50,
                data: function (params) {
                    return {
                        q: params.term,
                        page: params.page,
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
}

function CallRepMCUser() {
    if ($("#ddlMC") != null) {
        // Material Center
        $("#ddlMC").select2({
            placeholder: 'Search Material Center by Name or Code',
            minimumInputLength: 0,
            ajax: {
                url: "/MC/SearchMCUser",
                dataType: 'json',
                delay: 50,
                data: function (params) {
                    return {
                        q: params.term,
                        page: params.page,
                        x: "All"
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
}

function CallRepMC() {
    // Material Center
    $("#ddlMC").select2({
        placeholder: 'Search Material Center by Name or Code',
        minimumInputLength: 0,
        ajax: {
            url: "/MC/SearchMC",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "All"
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

function CallCustomer() {
    $("#ddlCustomer").select2({
        placeholder: 'Search Customer by Name or Code',
        minimumInputLength: 0,
        dropdownAutoWidth: true,
        ajax: {
            url: "/Customer/SearchCustomer",
            dataType: 'json',
            delay: 50,

            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0,
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
}

function CallCustomerNo() {
    $("#ddlCustomer").select2({
        placeholder: 'Search Customer by Name or Code',
        minimumInputLength: 0,
        ajax: {
            url: "/Customer/SearchCustomer",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0,
                    x: "No"
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
}

function CallCustomerAll() {
    $("#ddlCustomer").select2({
        placeholder: 'Search Customer by Name or Code',
        minimumInputLength: 0,
        ajax: {
            url: "/Customer/SearchCustomerAll",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0,
                    x: "No",
                    y: "All",
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
}

function CallCustomerBoth() {
    $("#ddlCustomer").select2({
        placeholder: 'Search Customer by Name or Code',
        minimumInputLength: 0,
        dropdownAutoWidth: true,
        ajax: {
            url: "/Customer/SearchCustomer",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0,
                    x: "Both"
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
}

function CallSupplier() {
    $("#ddlSupplier").select2({
        placeholder: 'Search Supplier by Name or Code',
        minimumInputLength: 0,
        ajax: {
            url: "/Supplier/SearchSupplier",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
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
}

function CallEmployee() {

    //bind to employee
    $("#ddlEmployee").select2({
        placeholder: 'Search by Name ',
        minimumInputLength: 0,
        ajax: {
            //url: "/Employee/SearchEmployee",
            url: "/Employee/SearchEmployeeUser",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0,
                    x: "empty"

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
}

function CallEmployeeAcc() {

    //bind to employee
    $("#ddlEmployee").select2({
        placeholder: 'Search by Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/Employee/SearchEmployeeWithAcc",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0,
                    x: "empty"

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
}

function CallSearchUser() {
    $("#ddlCreatedBy").select2({
        placeholder: 'Search User by Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/Users/SearchUser",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "all"
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

function ItemSearchable() {
    $("#ddlItem").select2({
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
function ItemSearchable2() {
    $("#ddlItem").select2({
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
        templateResult: repoFormatResult,
        templateSelection: repoFormatSelection,
    });
}
function repoFormatSelection(repo) {
    return repo.text;
}
function repoFormatResult(repo) {
    var bg = "";

    var markup = '<div class="se-row' + bg + '">' +
        '<h4>' + repo.text + '</h4>';

    if (repo.sprice)
        markup += '<div class="se-sec">Sales Price:' + parseFloat(repo.sprice).toFixed(2) + '</div>';
    if (repo.pprice)
        markup += '<div class="se-sec">Purchase Price:' + parseFloat(repo.pprice).toFixed(2) + '</div>';


    if (repo.cashprice)
        markup += '<div class="se-sec">Cash Price:' + parseFloat(repo.cashprice).toFixed(2) + '</div>';

    if (repo.creditprice)
        markup += '<div class="se-sec">Credit Price:' + parseFloat(repo.creditprice).toFixed(2) + '</div>';


    markup += '</div>';
    var retn = $(markup);
    return retn;
}
function CallEmployeeAcc() {

    //bind to employee
    $("#ddlEmployee").select2({
        placeholder: 'Search by Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/Employee/SearchEmployeeWithAcc",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0,
                    x: "empty"

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
}

function CallEmployeeAccAll() {

    //bind to employee
    $("#ddlEmployee").select2({
        placeholder: 'Search by Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/Employee/SearchEmployeeWithAcc",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0,
                    x: "all"

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
}

function CallEmployeeLeaveApp() {

    //bind to employee
    $("#ddlEmployee").select2({
        placeholder: 'Search by Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/Employee/SearchEmployeeLeaveApp",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0,
                    x: "empty"

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
}

function CallEmployeeFinalApp() {

    //bind to employee
    $("#ddlEmployee").select2({
        placeholder: 'Search by Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/Employee/SearchEmployeeFinalApp",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0,
                    x: "empty"

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
}

function CallEmployeeResinged() {

    //bind to employee
    $("#ddlEmployee").select2({
        placeholder: 'Search by Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/Employee/SearchEmployeeResigned",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0,
                    x: "empty"

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
}

function CallRepCustomer() {
    $("#ddlCustomer").select2({
        placeholder: 'Search Customer by Name or Code',
        minimumInputLength: 0,
        ajax: {
            url: "/Customer/SearchCustomer",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    x: "All",
                    page: params.page || 0,
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
}

function CallRepSupplier() {
    $("#ddlSupplier").select2({
        placeholder: 'Search Supplier by Name or Code',
        minimumInputLength: 0,
        ajax: {
            url: "/Supplier/SearchSupplier",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    x: "All",
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
}
function CallRepSupplier2() {
    $("#ddlSupplier").select2({
        placeholder: 'Search Supplier by Name or Code',
        minimumInputLength: 0,
        ajax: {
            url: "/Supplier/SearchSupplier2",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    x: "All",
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
}
function CallRepEmployee() {

    //bind to employee
    $("#ddlEmployee").select2({
        placeholder: 'Search by Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/Employee/SearchEmployee",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0,
                    x: "All"
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
}



function SelectToGroup(group) {
    var GroupName = (group.Name != null && group.Name != "") ? group.Name : "";
    var bg = "";
    var markup = "";
    markup = '<div class="se-row' + bg + '">';
    markup += '<div class="se-row"><h4>' + group.text + '</h4></div>' + //'+ group.text +' ,'+ group.Name +'
        '<div class="se-sec">' + GroupName + '</div>';
    markup += '</div>';
    var retn = $(markup);
    return retn;
}

function ToSetFormatSelection(group) {
    return group.text;
}

function projectFormatResult(repo) {
    var bg = "";
    bg = (parseFloat(repo.status) == 0) ? "" : " text-red";

    var markup = '<div class="se-row' + bg + '">' +
        '<h6>' + repo.text + '</h6>';
    markup += '</div>';
    var retn = $(markup);
    return retn;
}

function projectFormatSelection(repo) {
    return repo.text;
}
function CheckLoggedIn() {
    $.ajax({
        type: "POST",
        url: "/Home/loggedIn",
        success: function (data) {
            if (!data.v) {
                window.location = "/login"
            }
        }
    });
}
/* Stock Return Method */
function StockCalc(stock, ConFactor, PriUnit, SubUnit) {
    var stockIn;
    var primary = (stock / ConFactor);
    //if (stock % ConFactor == 0) {
    //    stockIn = (stock / ConFactor) + " " + PriUnit;
    //}
    //else {
    //    var p = parseInt(((stock / ConFactor) * 100) / 100);
    //    var sub = (stock % ConFactor).toFixed(0);
    //    stockIn = p + " " + PriUnit + ", " + sub + " " + SubUnit
    //}
    stockIn = (stock / ConFactor);
    return stockIn;
}

function CallProjectCustomerWise() {
    var customer = $('#ddlCustomer').val();
    $("#ddlProject").select2({
        placeholder: 'Search Project Code Or Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/Project/SearchProjectByCustomerOnly",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    customer: customer,
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
        templateResult: projectFormatResult,
        templateSelection: projectFormatSelection,
    });
}

function CallProject() {
    var customer = $('#ddlCustomer').val();
    $("#ddlProject").select2({
        placeholder: 'Search Project Code Or Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/Project/SearchProjectByCustomer",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    customer: customer,
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
        templateResult: projectFormatResult,
        templateSelection: projectFormatSelection,
    });
}
function CallProjectByAcc() {
    var account = $('#ddlpayfrom').val();
    $("#ddlProject").select2({
        placeholder: 'Search Project Code Or Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/Project/SearchProjectByAcc",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    account: account,
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
        //templateResult: projectFormatResult,
        //templateSelection: projectFormatSelection,
    });
}
function CallProjectAll() {
    $("#ddlProject").select2({
        placeholder: 'Search Project Code Or Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/Project/SearchAllProject",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "all"
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
function CallAllProjectByCustomer() {
    var customer = $('#ddlCustomer').val();
    $("#ddlProject").select2({
        placeholder: 'Search Project Code Or Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/Project/SearchAllProjectByCustomer",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "all",
                    customer: customer,
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
function CallProjectNew() {
    $("#ddlProject").select2({
        placeholder: 'Search Project Code Or Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/Project/SearchAllProject",
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

function CallTask() {

    var project = $('#ddlProject').val();
    $("#ddlProTask").select2({
        placeholder: 'Search Task Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/ProTask/SearchTaskByProject",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    project: project,
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
        // templateResult: projectFormatResult,
        // templateSelection: projectFormatSelection,
    });
}

function CallProjectEmpty() {
    var customer = $('#ddlCustomer').val();
    $("#ddlProject").select2({
        placeholder: 'Search Project Code Or Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/Project/SearchProjectByCustomer",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    customer: customer,
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
        templateResult: projectFormatResult,
        templateSelection: projectFormatSelection,
    });
}

function CallPOrder() {
    var supplier = $('#ddlSupplier').val();
    $("#ddlPOrder").select2({
        placeholder: 'Search Purchase Order',
        minimumInputLength: 0,
        ajax: {
            url: "/PurchaseOrder/SearchPOrder",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    supplier: supplier,
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

function CallApprovedBy() {
    $("#ddlApprovedBy").select2({
        placeholder: 'Search User',
        minimumInputLength: 0,
        ajax: {
            url: "/Employee/SearchEmployeeUser",
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

function CallApprovedBy2() {
    $("#ddlApprovedBy").select2({
        placeholder: 'Search User',
        minimumInputLength: 0,
        ajax: {
            url: "/Employee/SearchEmployeeUser2",
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

function CallTeam() {
    $("#AssignTypeAll").select2({
        placeholder: 'Search Assign Type',
        minimumInputLength: 0,
        ajax: {
            url: "/Team/SearchTeam",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0,
                    //x: "in"
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

//-----------------------------------------------------------------------------------------
//----------------------------------account reference-------------------------------------------
function bindAccRef(stype) {
    var table = '<table class="table table-bordered table-hover text-center" id="invoicedata">' +
        '<thead>' +
        '<tr class="bg-gray">' +
        '<th class="text-center">#</th><th class="text-center">Invoice No / Name</th>' +
        '<th class="text-center">Date</th>' +
        '<th class="text-center">Amount</th>' +
        '<th class="text-center">Action</th>' +
        '</tr>' +
        '</thead>' +
        '<tbody id="addinvoiceItem"></tbody></table>';
    $('#refdiv').html(table);

    addrowref('addinvoiceItem', 'new', '', '0.00', '', stype);
    $("#refdiv").show();
}

var count = 1, type = '';
limits = 500; var Amt = 0;
function addrowref(t, action, Invoice, Amount, Date, stype) {

    tabindex = count * 5;
    var slno = $('#addinvoiceItem tr').length + 1;
    var row = "<tr class='invoice_' id='invoice_" + count + "'>";
    var divid = "invoice_no_" + Invoice;

    if (Invoice != '') {
        row = "<tr class='invoice_" + count + "' id='invoice_" + count + "'>";
    }

    tab1 = tabindex + 1;
    tab2 = tabindex + 2;
    tab3 = tabindex + 3;
    tab4 = tabindex + 4;

    Date = Date != "" ? convertToDate(Date) : "";
    stype = '"' + stype + '"';

    data = "<td class='text-center' id=" + divid + "> " + slno + " </td>" +
        "<td><input type='text' data-name='Invoice' name='invoicedata[" + (slno - 1) + "].Invoice' onchange='invoice_no_changeref(" + count + "," + stype + ");' id='invoice_no_" + count + "'  class='invoice_no_" + count + " form-control text-center invno' tabindex='" + tab1 + "' value='" + Invoice + "' /></td>" +
        "<td><input type='text' data-name='RADate' name='invoicedata[" + (slno - 1) + "].RADate' onchange='date_invoice_changeref(" + count + "," + stype + ");' id='radate_" + count + "'  class='radate_" + count + " form-control text-center date' tabindex='" + tab2 + "' value='" + Date + "' /></td>" +
        "<td><input type='number' data-name='Amount' name='invoicedata[" + (slno - 1) + "].Amount' onchange='amt_invoice_changeref(" + count + "," + stype + ");' id='amt_invoice_" + count + "' value='" + parseFloat(Amount).toFixed(2) + "'  class='amt_invoice_" + count + " form-control text-center invamt' placeholder='0' min='0' tabindex='" + tab3 + "' /></td>" +
        "<td><button type='button' tabindex='" + tab4 + "' style='text-align: right;' class='btn btn-danger'  value='Delete' onclick='deleteRowRef(this)'><i class='fa fa-trash fa-1x'></i></button> " +
        "</td>";
    row += data + "</tr>";
    $('#' + t).append(row);


    $('.date').datepicker({
        format: 'dd-mm-yyyy',
        autoclose: true,
        allowInputToggle: true
    });
    jQuery.validator.methods["date"] = function (value, element) { return true; }


    count++;
    setTabIndexRef();
}
function amt_invoice_changeref(arg, stype) {
    $("#amt_invoice_" + arg).closest('tr').attr('class', "invoice_" + arg);


    var invno = $("#invoice_no_" + arg).val();
    var invdate = $("#radate_" + arg).val();
    var invamt = $("#amt_invoice_" + arg).val();


    //--------check empty rows----------------------------------------
    var count = 0;
    $("#addinvoiceItem tr").each(function () {
        var classname = $(this).closest('tr').attr('class');
        if (classname == 'invoice_') {
            count++;
        }
    });

    if (count == 0 && (invno != "" && invdate != "" && invamt != ""))
        addrowref('addinvoiceItem', 'new', '', '0.00', '', stype);
    //-------------------------------------------------------------------

    RowTotalRef();
}
function invoice_no_changeref(arg, stype) {
    var invno = $("#invoice_no_" + arg).val().trim();
    var tbody = $("#invoicedata tbody");
    var count = 0;

    var url = "";
    if (stype == "cust") {
        url = '/CreditSale/chkBillExist';
    }
    if (stype == "supp") {
        url = '/PurchaseEntry/chkBillExist';
    }

    $.ajax({
        url: url,
        type: "GET",
        dataType: "JSON",
        data: { SENo: invno },
        success: function (result) {
            if (result) {
                alert("invoice No. Already Exists..!!");
                $("#invoice_no_" + arg).val("");
            }
        }
    });

    if (tbody.children().length > 0) {
        tbody.children("tr").each(function () {
            var rowid = $(this).attr("id");
            var billno = $("#" + rowid + " .invno").val();
            if (invno == billno) {
                count++;
            }
        });
    }

    if (count > 1) {
        alert("invoice No. Already Exists..!!");
        $("#invoice_no_" + arg).val("");
    }

    amt_invoice_changeref(arg, stype);
}
function date_invoice_changeref(arg, stype) {
    amt_invoice_changeref(arg, stype);
}


function deleteRowRef(t, item) {
    var classname = $(t).closest('tr').attr('class');
    if (classname == 'invoice_') alert("Sorry you can't delete this row.");
    else {
        var e = t.parentNode.parentNode;
        e.parentNode.removeChild(e);
    }
    var i = 1;
    $('#addinvoiceItem tr').each(function () {
        $(this).find('td:first').text(i);
        $(this).find('input,select').each(function (colIndex, c) {
            var Tid = $(this).attr('id').split("_")[0];
            switch (Tid) {
                case "invoice": $(this).attr("name", "invoicedata[" + (i - 1) + "].Invoice");
                    break;
                case "radate": $(this).attr("name", "invoicedata[" + (i - 1) + "].RADate");
                    break;
                case "amt": $(this).attr("name", "invoicedata[" + (i - 1) + "].Amount");
                    break;
                default:
                    break;
            }
        });

        i++;
    });
    RowTotalRef();
}

function RowTotalRef() {
    var tbody = $("#invoicedata tbody");
    if (tbody.children().length > 0) {
        var totAmt = 0;
        $(".invamt").each(function () {
            var Amt = this.value;
            Amt = Amt || 0;
            totAmt = parseFloat(totAmt) + parseFloat(Amt);
        });
        $("#OpnBalance").val(totAmt.toFixed(2));
    }
}

function setTabIndexRef() {
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

function ProjectPopup() {
    $('#modal-project').on('shown.bs.modal', function (e) {

        var date = new Date();
        date.setDate(date.getDate());
        $('.date').datepicker({
            startDate: date,
            format: 'dd-mm-yyyy',
            autoclose: true,
            allowInputToggle: true
        });
        jQuery.validator.methods["date"] = function (value, element) { return true; }

        //$(function () {
        //    $(".textareapopup").wysihtml5();
        //});

        // $("#ddlProType").select2();








        $("#ddlCust").select2({
            placeholder: 'Search Customer by Name or Code',
            minimumInputLength: 0,
            ajax: {
                url: "/Customer/SearchLeadPipeCust",
                dataType: 'json',
                delay: 50,
                data: function (params) {
                    return {
                        q: params.term || "",
                        page: params.page || 0,
                        x: "No"
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

        $("#SalesPerson").select2({
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


        $('#StartDate').datepicker({
            format: 'dd-mm-yyyy',
            autoclose: true,
            allowInputToggle: true
        });
        $('#EndDate').datepicker({
            format: 'dd-mm-yyyy',
            autoclose: true,
            allowInputToggle: true
        });
        var datetd = new Date();
        datetd.setDate(datetd.getDate());
        var datenew = new Date();
        datenew.setDate(datenew.getDate());



        $('body').on('change', '#StartDate', function (e) {
            var date = $(this).datepicker('getDate');
            if (date) {
                date.setDate(date.getDate());
            }
            // $('#ExEndDate').datepicker("setDate", date);
            $('#EndDate').datepicker("setStartDate", date);
        });



        $('body').on('change', '#ddlCust', function (e) {
            var custId = $(this).val();
            if (custId != "") {
                $.ajax({
                    url: '/Customer/SearchCustomer',
                    type: "GET",
                    dataType: "JSON",
                    data: { custId: custId },
                    success: function (result) {
                        if (result != null) {
                            $('#Location').val(result.Location);
                            $('#ContactPerson').val(result.ContactPerson);
                        }
                    }
                })
            }
        });


        $.fn.modal.Constructor.prototype.enforceFocus = function () { };
        $('#modal-create').on('shown.bs.modal', function (e) {

        });

        $('span').on('click', '.modal-create', function (e) {
            e.preventDefault();
            var url = $(this).attr('href');
            modalshow(url, '#modal-create');
            //datepickerInit();
        });
        $('#modal-create').on('submit', '#createform', function (e) {
            e.preventDefault();
            var url = $('#createform')[0].action;
            var data = $('#createform').serialize();
            createajax(url, data, '#modal-create');
        });

        $('span').on('click', '.modal-create', function (e) {
            e.preventDefault();
            var url = $(this).attr('href');
            modalshow(url, '#modal-create');
            //datepickerInit();
        });
        $('#modal-create').on('submit', '#createform', function (e) {
            e.preventDefault();
            var url = $('#createform')[0].action;
            var data = $('#createform').serialize();
            createajax(url, data, '#modal-create');
        });


        $('span').on('click', '.modal-ptype', function (e) {
            e.preventDefault();
            var url = $(this).attr('href');
            modalshow(url, '#modal-ptype');
        });

        $('#modal-ptype').on('submit', '#typeform', function (e) {
            var url = $('#modal-ptype #typeform')[0].action;

            var text = $("#TypeName").val();
            $('#ddlProType option:selected').attr("selected", null);
            $.ajax({
                type: "POST",
                url: url,
                data: $('#modal-ptype #typeform').serialize(),
                success: function (data) {
                    if (data.status) {
                        $('#modal-ptype').modal('hide');

                        var newOption = $('<option></option>');
                        newOption.val(data.Id).attr("selected", "selected");
                        newOption.html(text);
                        $('#ddlProType').append(newOption);
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
            $('#modal-ptype').modal('hide');
            $('#modal-ptype').removeData('bs.modal');
            $("button").prop('disabled', true);
        });



        $('span').on('click', '.modal-customer', function (e) {
            e.preventDefault();
            var url = $(this).attr('href');
            modalshow(url, '#modal-customer');
            //datepickerInit();
        });
        $('#modal-customer').on('submit', '#createform', function (e) {
            e.preventDefault();
            var url = $('#createform')[0].action;
            var data = $('#createform').serialize();
            createajax(url, data, '#modal-customer');
        });

        $('div').on('click', '.modal-addnew', function (e) {
            e.preventDefault();
            $(this).attr('data-target', '#modal-addnew');
            $(this).attr('data-toggle', 'modal');
        });
        $('#modal-addnew').on('submit', '#pstatform', function (e) {
            var url = $('#modal-addnew #pstatform')[0].action;

            var text = $("#StatusName").val();
            $('#ddlPStatus option:selected').attr("selected", null);
            $.ajax({
                type: "POST",
                url: url,
                data: $('#modal-addnew #pstatform').serialize(),
                success: function (data) {
                    if (data.status) {
                        $('#modal-addnew').modal('hide');

                        var newOption = $('<option></option>');
                        newOption.val(data.Id).attr("selected", "selected");
                        newOption.html(text);
                        $('#ddlPStatus').append(newOption);
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
            $('#modal-addnew').modal('hide');
            $('#modal-addnew').removeData('bs.modal');
            $("button").prop('disabled', true);
        });
    });


    $('#modal-project').on('submit', '#projectform', function (e) {
        e.preventDefault();
        if ($("#projectform").valid()) {
            var url = $('#projectform')[0].action;
            var formData = new FormData(this);
            $.ajax({
                async: true,
                cache: false,
                dataType: "json",
                type: "POST",
                processData: false,
                contentType: false,
                url: url,
                data: formData,
                beforeSend: function () {
                    $("button").prop('disabled', true); // disable button
                },
                success: function (e) {
                    if (e.status) {
                        $('.ajax_response', res_success).text(e.message);
                        $('.AlertDiv').prepend(res_success);
                        $('#modal-project').modal('hide');
                        $('#modal-project').removeData('bs.modal');
                    }
                    else {
                        $('.ajax_response', res_danger).text(e.message);
                        $('.AlertDiv').prepend(res_danger);
                        // $("button").prop('disabled', false); // enable button
                    }
                    $("button").prop('disabled', false);
                }
            });
        }
    });

}

function CustomerPopup() {
    $('#modal-create').on('shown.bs.modal', function (e) {
        $("#ddlSalesPerson").select2({
            placeholder: 'Search by Name ',
            minimumInputLength: 0,
            ajax: {
                url: "/Employee/SearchEmployee",
                dataType: 'json',
                delay: 50,
                data: function (params) {
                    return {
                        q: params.term || "",
                        page: params.page || 0,
                        x: "empty"

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
        $("#TaxType").select2();

        $("#ddlSrcLead").select2({
            placeholder: 'Search Source Of Lead Name',
            minimumInputLength: 0,
            ajax: {
                url: "/SourceOfLead/SearchSrcLead",
                dataType: 'json',
                delay: 50,
                data: function (params) {
                    return {
                        q: params.term,
                        page: params.page,
                        x: "empty",
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

        SearchCustExists();

        $('div').on('click', '.modal-src', function (e) {
            e.preventDefault();
            $(this).attr('data-target', '#modal-src');
            $(this).attr('data-toggle', 'modal');
        });

        $('#modal-src').on('submit', '#createform', function (e) {
            var url = $('#modal-src #createform')[0].action;
            var text = $("#SrcName").val();
            $('#modal-src option:selected').attr("selected", null);
            $.ajax({
                type: "POST",
                url: url,
                data: $('#modal-src #createform').serialize(),
                success: function (data) {
                    if (data.status) {
                        $('#modal-src').modal('hide');
                        var newOption = $('<option></option>');
                        newOption.val(data.Id);//attr("selected", "selected");
                        newOption.html(text);
                        $('#ddlSrcLead').append(newOption);
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

        $.fn.modal.Constructor.prototype.enforceFocus = function () { };
        $('#modal-create').on('shown.bs.modal', function (e) {

        });


        $('body').on('change', '#DC', function (e) {
            if ($(this).val() == 0 && '@ViewBag.BillToReceipt' == '0') {//debit
                $("#refdiv").show();
                bindAccRef("cust");
            } else {
                $("#refdiv").hide();
            }
        });
        AddMobile();
        //if ('#BillToReceipt' == '0') {
        //    bindAccRef("cust");
        //}

    });

}

function isTicked() {
    //check "select all" if all checkbox items are checked
    if ($('.checkbox:checked').length == $('.checkbox').length && ($('.checkbox:checked').length > 0)) {
        $("#select_all").prop('checked', true);
    }
    else {
        $("#select_all").prop('checked', false);
    }
}

function LastUpdatedIn(data) {

    var datas = data;
    // alert(datas);
    var display = "";
    if (datas != "") {
        var normal = convertDateTime(datas);
        // var ldat1 = convertDateTime(datas, "YYYY-MM-DD hh:mm");
        var ldat1 = convertDateTimeYMD(datas);
        var ldat = new Date(ldat1);
        //var newDate = new Date();
        //var cdat = new Date(newDate.toLocaleString());
        var cdat = Date.now();
        var hours = Math.abs(ldat - cdat) / 36e5;
        if (hours < 24) {
            if (hours > 1) {
                display = Math.round(hours) + "Hrs Ago";
            }
            else {
                var mins = Math.round(hours * 60);
                display = mins + " Mins Ago";

            }
        }
        else {
            var delta = Math.abs(ldat - cdat) / 1000;
            var days = Math.floor(delta / 86400);
            display = (days == 1) ? days + " Day Ago" : days + " Days Ago";
            // display = Math.ceil(hours / (1000 * 60 * 60 * 24)) + "Day Ago";
        }
        // display = ldat1;
    }

    return display;
}

function chkEditEditable(chkapp) {
    if (chkapp == "False") {
        $("#save").prop('disabled', true);
        $("#sendmail").prop('disabled', true);
        $("#AlertEditable").text("- Not Editable");
        $("#print").html("Print");
    }
}

function SaleHireEntry(stype) {
    //bind to salesentry
    $("#ddlSalesEntry").select2({
        placeholder: 'Search Sales Entry ',
        minimumInputLength: 0,
        ajax: {
            url: "/CreditSale/SearchSaleEntry",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params,
                    stype: stype
                };
            },
            processResults: function (data, params) {
                params.page = params.page || 0;
                return {
                    results: data,
                    pagination: {
                        more: true
                    }
                };
            },
            cache: true
        },
    });
}
function CallTaskStatus() {
    $("#TaskStat").select2({
        placeholder: 'Search Task Status ',
        minimumInputLength: 0,
        ajax: {
            url: "/TaskStatus/SearchTaskStatus",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "all"
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
function CallleadStatus2() {
    alert("test");
    $("#ddlPStatus").select2({
        placeholder: 'Search lead Status ',
        minimumInputLength: 0,
        ajax: {
            url: "/TaskStatus/SearchleadStatus",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "all"
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

function CallTaskName() {
    //task name
    $("#ddlTaskName").select2({
        placeholder: 'Search Task Name',
        minimumInputLength: 0,
        ajax: {
            url: "/ProTask/SearchProTask",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "all"

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

function CallTaskNameTag() {
    $("#TaskName").select2({

        tags: true,
        minimumInputLength: 0,
        ajax: {
            url: "/ProTask/SearchProTaskTag",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
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


function CallTaskType() {
    //tasktype
    $("#ddlTaskType").select2({
        placeholder: 'Search Task Type',
        minimumInputLength: 0,
        ajax: {
            url: "/ProTaskType/SearchTaskType",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "all"

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

function CallLeadPipeCust() {
    $("#ddlCustomer").select2({
        placeholder: '',
        minimumInputLength: 0,
        ajax: {
            url: "/Customer/SearchLeadPipeCust",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0,
                    x: "No"
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
        templateResult: SelectToGroup,
        templateSelection: ToSetFormatSelection,
    });
}

function CallLeadPipeCustAll() {
    $("#ddlCustomer").select2({
        placeholder: '',
        minimumInputLength: 0,
        dropdownAutoWidth: true,
        ajax: {
            url: "/Customer/SearchLeadPipeCustAll",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0,
                    x: "No",
                    y: "All",
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
        templateResult: SelectToGroup,
        templateSelection: ToSetFormatSelection,
    });
}

function CallTaskStatus() {
    // Item Category
    $("#ddlPStatus").select2({
        placeholder: 'Search Task Status by Name',
        minimumInputLength: 0,
        ajax: {
            url: "/TaskStatus/SearchTaskStatus",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "default"
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
function TaskStatusName() {
    $("#TaskStatus").select2({
        placeholder: 'Search Task Status',
        minimumInputLength: 0,
        ajax: {
            url: "/TaskStatus/SearchTaskStatusName",
            dataType: 'json',
            type: "POST",
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
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

function AssignTypeChange(assign) {

    if (assign != null && assign != "") {
        var myObj = {};
        $.each(assign, function (i, value) {
            myObj[i] = value;
        });
        //if (assign != -1) {
        $.ajax({
            url: '/Team/GetAllMembers',
            type: "GET",
            dataType: "JSON",
            data: { assign: myObj },
            success: function (result) {
                if (result != null) {
                    var chk = 0;
                    var values = new Array();
                    $.each(result, function (j, member) {
                        if (chk != member.lead) {
                            values.push(member.lead);
                            chk = member.lead;
                        }
                        values.push(member.emp);
                        chk++;
                    });
                    $('#AssignedUsers').val(values).trigger("change");
                } else {
                    $('#AssignedUsers').val(null).trigger("change");
                }
            }
        });
        // $("#AssignedUsers").prop("disabled", false);
        //CallTaskStatus();
        //}
        //else {
        //    // $("#AssignedUsers").prop("disabled", true);
        //    $('#AssignedUsers').val(null).trigger('change');
        //}
    }
    else {
        // $("#AssignedUsers").prop("disabled", true);
        $('#AssignedUsers').val(null).trigger('change');
    }
}

function CallBankAcctPDC2() {
    $("#ddlBank").select2({
        placeholder: 'Search by Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/Accounts/SearchAccountsPDC2",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0,
                    x: "empty"

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
}
function CallBankAcctPDC() {
    $("#ddlBank").select2({
        placeholder: 'Search by Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/Accounts/SearchAccountsPDC",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0,
                    x: "empty"

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
}
function SrcOfLead() {
    $("#ddlSrcLead").select2({
        placeholder: 'Search Source Of Lead Name',
        minimumInputLength: 0,
        ajax: {
            url: "/SourceOfLead/SearchSrcLead",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "empty",
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

function CallLeadStatus() {
    $("#ddlLeadStat").select2({
        placeholder: 'Search Lead Status Name',
        minimumInputLength: 0,
        ajax: {
            url: "/LeadStatus/SearchLeadStatus",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "empty",
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

function GetProLocation() {
    $("#ddlLocation").select2({
        placeholder: 'Search Location',
        minimumInputLength: 0,
        ajax: {
            url: "/Project/SearchProjectLocation",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "all"

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
function GetTaskLocation() {
    $("#ddlLocation").select2({
        placeholder: 'Search Location',
        minimumInputLength: 0,
        ajax: {
            url: "/ProTask/Location",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "all"

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

function SearchAllMobile() {
    $("#ddlmobile").select2({

        placeholder: 'Search Mobile/Phone',
        minimumInputLength: 0,
        ajax: {
            url: "/Customer/SearchAllMobileAndPhone",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "all"

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

function CallTaskMobile() {

    var custId = $("#ddlCustomer").val();

    $.ajax({
        url: "/Customer/SearchAllMobileAndPhonebyID",
        type: "GET",
        dataType: "JSON",
        data: { cust: custId },
        success: function (data) {
            if (data != null) {
                $("#mobiles").html('');
                leng = data.length;
                if (leng > 0) {
                    $.each(data, function (i, item) {
                        AddMobile(item.text, item.Name);
                    });
                } else {
                    AddMobile();
                }
            }
        }
    });



    //var custId = $("#ddlCustomer").val();
    //$('#TaskMobiles').val(null).trigger("change");
    //$("#TaskMobiles option").remove();

    //$.ajax({
    //    url: "/Customer/SearchAllMobileAndPhonebyID",
    //    type: "GET",
    //    dataType: "JSON",
    //    data: { cust: custId },
    //    success: function (result) {
    //        if (result != null) {
    //            var values = new Array();
    //            $.each(result, function (j, member) {
    //                if (member.text != null && member.id != null) {
    //                    var newOption = new Option(member.text, member.id, true, true);
    //                    values.push(newOption);
    //                }
    //            });
    //            $('#TaskMobiles').append(values).trigger('change');
    //        } else {
    //            $('#TaskMobiles').val(null).trigger("change");
    //        }
    //    }
    //});

}

function SearchTaskMobile() {
    $("#ddlmobile").select2({

        placeholder: 'Search Mobile/Phone',
        minimumInputLength: 0,
        ajax: {
            url: "/ProTask/SearchAllMobileAndPhone",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "all"

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
function SearchitemcodeExists() {
    $("#ItemCode").select2({
        tags: true,
        minimumInputLength: 0,
        ajax: {
            url: "/Item/SearchitemcodeCheck",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "all"
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
function SearchitemExists() {
    $("#ItemName").select2({
        tags: true,
        minimumInputLength: 0,
        ajax: {
            url: "/Item/SearchitemCheck",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "all"
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
function SearchCustExists() {
    $("#CustomerName").select2({
        tags: true,
        minimumInputLength: 0,
        ajax: {
            url: "/Customer/SearchCustCheck",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "all"
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

//--------------------------------------------------------------------------------------------
//-----------------------------------------------------------------------------------------------

function GetWorkShift() {
    $("#WorkShift").select2({
        placeholder: 'Search Work Shift Name',
        minimumInputLength: 0,
        ajax: {
            url: "/Hr/WorkShift/SearchWorkShift",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "empty",
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
function GetEmpGrade() {
    $("#EmployeeGrade").select2({
        placeholder: 'Search Employee Grade Name',
        minimumInputLength: 0,
        ajax: {
            url: "/Hr/EmployeeGrade/SearchEmployeeGrade",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "empty",
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

function GetAllAccounts(val) {
    var type = val = 0 ? "empty" : "all";
    $("#ddlAccount").select2({
        placeholder: 'Search Account By Name',
        minimumInputLength: 0,
        ajax: {
            //url: "/Accounts/SearchAccounts",
            url: "/Accounts/AllAccounts",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0,
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
        templateResult: SelectToGroup,
        templateSelection: ToSetFormatSelection,
    });
}

function GetDepartment() {
    $("#ddlDepartment").select2({
        placeholder: 'Search by Name',
        minimumInputLength: 0,
        ajax: {
            url: "/Department/SearchDepartment",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0,
                    x: "all"
                };
            },
            processResults: function (data, params) {
                params.page = params.page || 0;
                return {
                    results: data,
                    pagination: {
                        more: true
                    }
                };
            },
            cache: true
        },
    });
}

function CallEmployeeByDept(dept) {

    //bind to employee
    $("#ddlEmployee").select2({
        placeholder: 'Search by Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/Employee/SearchEmployeeByDept",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0,
                    dept: dept
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
}



function getNatural(num) {
    return parseFloat(num.toString().split(".")[0]);
}
function getFraction(num) {
    return parseFloat(num.toString().split(".")[1]);
}
//function TxtTagCall() {
//    $("#Ref1").select2({
//        placeholder: 'Search by Name',
//        minimumInputLength: 0,
//        ajax: {
//            url: "/ProTask/SearchTag",
//            dataType: 'json',
//            delay: 50,
//            data: function (params) {
//                return {
//                    q: params.term || "",
//                    x: "empty",
//                    page: params.page || 0,
//                };
//            },
//            processResults: function (data, params) {
//                params.page = params.page || 0;
//                return {
//                    results: data,
//                    pagination: {
//                        more: true
//                    }
//                };
//            },
//            cache: true
//        },
//    });
//}
function callbank() {
    $("#Bank").select2({
        placeholder: 'Search by Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/Accounts/SearchAccountsPDC2",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0,
                    x: "empty"

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
}
function Setdate() {
    var str = maxDate.split("/");
    var str1 = minDate.split("/");
    var date = new Date();
    var today;
    var priordate; //alert(parseFloat(str[2]) - parseFloat(str1[2]))
    if (parseFloat(str[2]) - parseFloat(str1[2]) == 4) {
        today = new Date(date.getFullYear(), date.getMonth(), date.getDate());
        priordate = new Date(new Date().setDate(today.getDate() - 30));
    }
    else {
        today = new Date(date.getFullYear(), date.getMonth(), date.getDate());
        priordate = new Date(new Date().setDate(today.getDate() - 30));
        var c = today.toString();
        c = c.split("/");
        var end = new Date(str[2], parseInt(str[1]) - 1, str[0]);  // -1 because months are from 0 to 11
        var start = new Date(str1[2], parseInt(str1[1]) - 1, str1[0]);
        var check = new Date(c[2], parseInt(c[1]) - 1, c[0]);
        if (check > start && check < end) {
            today = new Date(date.getFullYear(), date.getMonth(), date.getDate());
            priordate = new Date(new Date().setDate(today.getDate() - 30));
        } else {
            priordate = new Date(minDate);
            today = new Date(minDate);
        }
    }
}
function detectmob() {
    if (window.innerWidth <= 800 || window.innerHeight <= 600) {
        return true;
    } else {
        return false;
    }
}
