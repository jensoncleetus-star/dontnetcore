
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
    var url = $('#recform')[0].action;
    var data = $('#recform').serialize();
    var payfrom = $("#ddlpayfrom").val();
    var payto = $("#ddlpayto").val();
    if (payfrom != payto) {
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
                        jrnlprint(data.data);
                    }
                    else {
                        $('.ajax_response', res_success).text(data.message);
                        $('.AlertDiv').prepend(res_success);
                    }
                    if (url == null) {
                        window.location.href = '/Journal/Create';
                    } else {
                        location.reload();
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
    else {
        alert("Debit and Credit Accounts Not to be same")
    }
}
function jrnlprint(data) {
    $("[id$=lblBillNo]").text(data.VoucherNo);
    $("[id$=lblDate]").text(data.Date);

    $("[id$=lblcreditor]").text(data.creditor);
    $("[id$=lblcrediamt]").text(data.Paying.toFixed(2));

    $("[id$=lbldebtor]").text(data.debitor);
    $("[id$=lbldebitamt]").text(data.Paying.toFixed(2));

    $("[id$=lbldebitsum]").text(data.Paying.toFixed(2));
    $("[id$=lblcreditsum]").text(data.Paying.toFixed(2));


    var originalpage = document.body.innerHTML;
    var printContent = $('#printit').html();
    $('title').html("Journal Voucher - " + data.VoucherNo);
    $('body').html(printContent);
    window.print();
}

// Common Methods

function JournalFromTo() {
    // paid to atribute
    $("#ddlpayfrom,#ddlpayto").select2({
        placeholder: 'Search Account By Code or name',
        minimumInputLength: 0,
        ajax: {
            url: "/Accounts/AllAccounts",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0,
                    z: ""
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
            $('#ddlpayfrom').val(null).trigger('change');
            $('.acdet').hide();
        }
    }
}
