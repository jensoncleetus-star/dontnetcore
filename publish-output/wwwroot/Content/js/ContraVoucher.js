// find account balance on select
function accbalance(accid) {
    if (accid != null && accid != 0) {
        $.ajax({
            async: true,
            cache: false,
            dataType: "json",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: "/Accounts/ChekSale",
            data: JSON.stringify({ id: accid }),
            success: function (data) {
                var amount = data.balance.amount.toFixed(2) + " " + data.balance.type;                
                $('#accdetails').text(amount);                
                $('#accbalance').show();
            }
        });
    }
}
// find account balance on select
function accbalanceto(accid) {
    if (accid != null && accid != 0) {
        $.ajax({
            async: true,
            cache: false,
            dataType: "json",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: "/Accounts/ChekSale",
            data: JSON.stringify({ id: accid }),
            success: function (data) {
                var amount = data.balance.amount.toFixed(2) + " " + data.balance.type;               
                $('#accdetailsto').text(amount);
                $('#accbalanceto').show();
            }
        });
    }
}
// form submition
function formsubmition(fnval) {
    var url = $('#conform')[0].action;
    var data = $('#conform').serialize();
    $.ajax({
        type: "POST",
        url: url,
        data: data,
        beforeSend: function () {
            $("button").prop('disabled', true); // disable button
        },
        success: function (data) {
            if (data.status) {
                if (data.type == 'print') {
                    recprint(data.data, data.fmapp);
                }
                else {
                    $('.ajax_response', res_success).text(data.message);
                    $('.AlertDiv').prepend(res_success);
                }
                if (fnval != null) {
                    window.location.href = '/ContraVoucher/Create';
                } else {
                    window.location.href = '/ContraVoucher/Index';
                }
            }
            else {
                $('.ajax_response', res_danger).text(data.message);
                $('.AlertDiv').prepend(res_danger);
                $("button").prop('disabled', false); // enable button
            }
        }
    })
}
function recprint(data, fmapp) {


    $("[id$=lblBillNo]").text(data.VoucherNo);
    $("[id$=lblDate]").text(data.Date);

    $("[id$=lblcreditor]").text(data.creditor);
    $("[id$=lblcrediamt]").text(data.Amount.toFixed(2));

    $("[id$=lbldebtor]").text(data.debitor);
    $("[id$=lbldebitamt]").text(data.Amount.toFixed(2));

    $("[id$=lbldebitsum]").text(data.Amount.toFixed(2));
    $("[id$=lblcreditsum]").text(data.Amount.toFixed(2));


    if (fmapp != null) {
        $.each(fmapp, function (i, mapp) {

            if (mapp.Field == "Ref1") {
                $("#IblRef1").text(mapp.FieldName);
                $("#IblRef1Val").text(data.Ref1);
                $("#divRef1").show();
            }
            if (mapp.Field == "Ref2") {
                $("#IblRef2").text(mapp.FieldName);
                $("#IblRef2Val").text(data.Ref2);
                $("#divRef2").show();
            }
            if (mapp.Field == "Ref3") {
                $("#IblRef3").text(mapp.FieldName);
                $("#IblRef3Val").text(data.Ref3);
                $("#divRef3").show();
            }
            if (mapp.Field == "Ref4") {
                $("#IblRef4").text(mapp.FieldName);
                $("#IblRef4Val").text(data.Ref4);
                $("#divRef4").show();
            }
            if (mapp.Field == "Ref5") {
                $("#IblRef5").text(mapp.FieldName);
                $("#IblRef5Val").text(data.Ref5);
                $("#divRef5").show();
            }
        });
    }

    var originalpage = document.body.innerHTML;
    var printContent = $('#printit').html();
    $('body').html(printContent);
    var titname = "Contra Voucher - " + data.debitor + " - " + data.VoucherNo;
    $('title').html(titname);
    window.print();
}

function PaymentFromTo() {
    // paid to atribute
    $("#ddlpayfrom, #ddlpayto").select2({
        placeholder: 'Search Account By Code or name',
        minimumInputLength: 0,
        ajax: {
            //url: "/Accounts/SearchAccounts",
            url: "/Accounts/SearchPaymentFrom",
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

function CheckFromToAcc() {
    var from = $('#ddlpayfrom').val();
    var to = $('#ddlpayto').val();

    if (from != 0 && to != 0) {
        if (from == to) {
            alert("Select Different Account..")
            $('#ddlpayto').val(null).trigger('change');
           // $('#ddlpayfrom').val(null).trigger('change');            
            $('.acdet').hide();
        }
    }
}