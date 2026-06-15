var count = 1, type = '';
limits = 500;
//Add generateditem Row ItemSubTotal GeneratedTotal action
function addrow(t, DrCr, Account, Debit, Credit, Narration, AccName, itemdata, Bsns) {
    if (count == limits) alert("You have reached the limit of adding " + count + " inputs");
    else {
        var OptionCrDr = "";
        var OptionAcc = "";
        var optionunit = "";
        var OptionPro = "";
        var OptionTsk = "";
        var required = "";
        var slno = $('#addinvoiceItem tr').length + 1;
        var row = "<tr class='item_' id='item_" + count + "'>";
        var data = "";
        var price = 0;
        var baseprice = 0;
        var mrp = 0;
        var htdata = "";
        tabindex = count * 5;
        tab1 = tabindex + 1;
        tab2 = tabindex + 2;
        tab3 = tabindex + 3;
        tab4 = tabindex + 4;
        tab5 = tabindex + 5;
        tab6 = tabindex + 6;


        var DCName = DrCr == 1 ? "Credit" : "Debit";
        if (Account != null && Account != 0) {
            row = "<tr class='item_" + Account + "' id='item_" + count + "'>";

            if (DrCr == 1) {
                OptionCrDr = "<option value='0'>Debit</option> <option value='1' selected>Credit</option>";
            } else {
                OptionCrDr = "<option value='0' selected>Debit</option> <option value='1'>Credit</option>";
            }

            OptionAcc = "<option value='" + Account + "'>" + AccName + "</option>";
        } else {
            OptionCrDr = "<option value='0'>Debit</option> <option value='1'>Credit</option>";
        }
        if (count == 1) {
            required = 'required="required"';
        }
        Narration = Narration != null ? Narration : "";
        // notbtn = "<button type='button' class='itnote btn btn-default btn-flat' data-toggle='modal' data-target='#modal-item-" + count + "'><i class='fa fa-1x fa-file-text-o'></i></button>";

        if (itemdata != null && itemdata.ProjectId != null && itemdata.ProjectName != null) {
            OptionPro = "<option value='" + itemdata.ProjectId + "'>" + itemdata.ProjectName + "</option>";
        }
        if (itemdata != null && itemdata.TaskId != null && itemdata.TaskName != null) {
            OptionTsk = "<option value='" + itemdata.TaskId + "'>" + itemdata.TaskName + "</option>";
        }
        var project_name = "";//(Bsns=="Property")?"property_name":"project_name";
        var task_name = "";
        if (Bsns == "Property") {
            project_name = "property_name";
            task_name = "unit_name";
        } else {
            project_name = "project_name";
            task_name = "task_name";
        }
        var itemaddbtn = "<span class='input-group-btn'><a type='button' href='/Accounts/Create' class='modal-create btn btn-success btn-flat'><i class='fa fa-1x fa-plus-circle'></i></a></span>";
        //  var itemaddbtn = "<span class='input-group-btn'><a type='button' href='/Accounts/Create' class='modal-create-lg btn btn-success btn-flat'><i class='fa fa-1x fa-plus-circle'></i></a></span>";
        data = "<td class='text-center'> " + slno + " </td>" +
            "<td><select data-name='AccType' class='form-control item_drcr_" + count + "' data-id='" + count + "' placeholder='Dr/Cr' id='drcr_" + count + "' onchange='chkdebitcredit(" + count + ")'>" + OptionCrDr + "</select></td>" +
            "<td class='input-group input-group-sm'><select data-name='AccountID' class='form-control account_name' " + required + " data-id='" + count + "' placeholder='Accounts' onchange='Accountchange(this," + count + ",\"" + type + "\",\"" + Bsns + "\")' id='account_name_" + count + "'  data-msg-required='The Account is required'>" + OptionAcc + "</select> " + itemaddbtn + "</td>" +
            "<td><input data-name='Debit' type='number' name='debit_rate[]' " + required + " onchange='debit_change(" + count + ",\"" + Bsns + "\");' id='price_debit_" + count + "' value='" + Debit.toFixed(2) + "' class='price_debit_" + count + " form-control text-right debitrate' placeholder='0.00' min='0' tabindex='" + tab2 + "'/></td> " +
            "<td><input data-name='Credit' type='number' name='credit_rate[]' " + required + " onchange='credit_change(" + count + ");' id='price_credit_" + count + "' value='" + Credit.toFixed(2) + "' class='price_credit_" + count + " form-control text-right creditrate' placeholder='0.00' min='0' tabindex='" + tab3 + "'/></td> " +
            "<td><input data-name='Narration' type='text' name='Narration[]' onchange='narration_change(" + count + ");' id='Narration_" + count + "' value='" + Narration + "' class='Narration_" + count + " form-control Narration' tabindex='" + tab5 + "'/></td> ";

        var prochk = $("#procheck").val();
        if (prochk == "active") {
            data += "<td><select data-name='ProjectId' class='form-control " + project_name + "' name='project_name' data-id='" + count + "' placeholder='Project' id='project_name_" + count + "'  onchange='GetProjectChange(this," + count + ",\"" + type + "\",\"" + Bsns + "\")'>" + OptionPro + "</select></td>";
            data += "<td><select data-name='TaskId' class='form-control " + task_name + "' data-id='" + count + "' name='task_name' placeholder='Task' id='task_name_" + count + "' onchange='GetTaskChange(this," + count + ",\"" + type + "\",\"" + Bsns + "\")'>" + OptionTsk + "</select></td>";
        }
        data += "<td class='text-center'><button tabindex='" + tab6 + "' style='text-align: right;' class='btn btn-danger' type='button' value='Delete' onclick='deleteRow(this)'><i class='fa fa-trash fa-1x'></i></button></td>";

        row += data + "</tr>";
        $('#' + t).append(row);
        searchAccount();
        chkdebitcredit(count, Account);

        count++;
        setTabIndex();
        if (Bsns == "Property") {
            GetProperty();
            GetUnit();
        } else {
            searchproject();
            searchtask(count);
        }


    }
}
function searchAccount() {
    var selecteditem = new Array();

    $(".account_name").each(function () {
        selecteditem.push($(this).val());
    });

    $(".account_name").select2({
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
function chkdebitcredit(arg, acc) {
    var crdr = $(".item_drcr_" + arg).val();
    if (acc == "" || acc == null) {
        if (crdr == 0) {
            $("#price_debit_" + arg).val("0.00").attr('readonly', false);
            $("#price_credit_" + arg).val("0.00").attr('readonly', true);
        } else {
            $("#price_credit_" + arg).val("0.00").attr('readonly', false);
            $("#price_debit_" + arg).val("0.00").attr('readonly', true);
        }
    } else {
        if (crdr == 0) {
            $("#price_debit_" + arg).attr('readonly', false);
            $("#price_credit_" + arg).val("0.00").attr('readonly', true);
        } else {
            $("#price_credit_" + arg).attr('readonly', false);
            $("#price_debit_" + arg).val("0.00").attr('readonly', true);
        }
    }
    rowSubTotal();
}
function vatinputcheck(arg, Bsns) {

    var crdr = $(".item_drcr_" + arg).val();
    if (crdr == 0) {
        var dbprice = $("#price_debit_" + arg).val();
        var vatnature = $("#VATNature").val();
        if (dbprice > 0 && vatnature == 1) {
            var tbody = $("#normalinvoice tbody");
            if (tbody.children().length > 0) {
                tbody.children("tr").each(function () {
                    var rowid = $(this).attr("class");
                    if (rowid == 'item_') {
                        $(this).closest("tr").remove();
                    }
                });
            }

            var vatinput = parseFloat(dbprice) * .05;
            addrow('addinvoiceItem', 0, 501, vatinput, 0, "", "VAT Input", null, Bsns);
            addrow('addinvoiceItem', '', '', 0.00, 0.00, '', '', 0.00, Bsns);
        }
    }

}
//item details
function Accountchange(selectObject, dataid, action, Bsns) {
    if (selectObject.value) {
        var ItemId = selectObject.value;
        var prochk = $("#procheck").val();
        var project = $("#project_name_" + dataid).val();
        var task = $("#task_name_" + dataid).val();
        var account = $("#account_name_" + dataid).val();
        var flag = true;
        var accjnl = $("#accjnlcheck").val();

        if (ItemId != null) {
            $(selectObject).closest('tr').attr('class', "item_" + ItemId);
            if (prochk == "active") {
                if (project != null) {
                    var tbody = $("#normalinvoice tbody");
                    if (tbody.children().length > 2) {
                        tbody.children("tr").each(function () {
                            var rowid = $(this).attr("id");
                            var arr = rowid.split('_');
                            var arg = arr[1];
                            var proj = $("#" + rowid + " .project_name").val();
                            var acc = $("#" + rowid + " .account_name").val();
                            var tsk = $("#" + rowid + " .task_name").val();

                            if (dataid != arg && account == acc && project == proj && task == tsk) {
                                flag = false;
                            }
                        });
                    }
                }
                if (accjnl == "inactive" && account != null && flag == false) {
                    alert("You Cannot Add same Account More than 1 Times With Same Project And Task..!!");
                    $(selectObject).val(null).trigger('change');
                } else {
                    if ($(".item_").length == 0) {
                        addrow('addinvoiceItem', '', '', 0.00, 0.00, '', '', 0.00, Bsns);
                    }
                }
            } else {
                if (accjnl == "inactive") {
                    if ($(".item_" + ItemId).length == 1) {
                        $(selectObject).closest('tr').attr('class', "item_" + ItemId);
                        addrow('addinvoiceItem', '', '', 0.00, 0.00, '', '', 0.00, Bsns);
                    } else {
                        alert("You Cannot Add same Account More than 1 Times");
                        $(selectObject).val(null).trigger('change');
                    }
                } else {
                    addrow('addinvoiceItem', '', '', 0.00, 0.00, '', '', 0.00, Bsns);
                }
            }

        }
    }
    rowSubTotal();
}



function debit_change(arg, Bsns) {
    if ($("#account_name_" + arg).val() != null) {
        rowSubTotal();
    }
    else {
        $("#price_debit_" + arg).val(0);
        $("#price_credit_" + arg).val(0);
    }
    vatinputcheck(arg);
}
function credit_change(arg) {
    if ($("#account_name_" + arg).val() != null) {
        rowSubTotal();
    }
    else {
        $("#price_debit_" + arg).val(0);
        $("#price_credit_" + arg).val(0);
    }
}
function narration_change(arg) {
    if ($("#account_name_" + arg).val() == null) {
        $("#Narration_" + arg).val("");
    }
}
function rowSubTotal() {
    var drTotal = 0;
    var crTotal = 0;

    $(".debitrate").each(function () {
        var dtot = $(this).val();
        drTotal = parseFloat(drTotal) + parseFloat(dtot);
    });

    $(".creditrate").each(function () {
        var ctot = $(this).val();
        crTotal = parseFloat(crTotal) + parseFloat(ctot);
    });

    $("#total_credit").text((crTotal).toFixed(2));
    $("#total_debit").text((drTotal).toFixed(2));

}


function searchproject() {
    $(".project_name").select2({
        placeholder: 'Search Project Code Or Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/Project/SearchProject",
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
function searchtask(dataid) {
    var project = $("#project_name_" + dataid).val();
    $("#task_name_" + dataid).val(null).trigger('change');
    $(".task_name").select2({
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
function GetProperty(val) {
    var type = val = 0 ? "empty" : "all";
    $(".property_name").select2({
        placeholder: 'Search Property Name or Code',
        minimumInputLength: 0,
        ajax: {
            url: "/Property/PropertyMain/SearchProperty",
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
function GetUnit(val) {
    $(".unit_name").select2({
        placeholder: 'Search Unit by Name or Code',
        minimumInputLength: 0,
        ajax: {
            url: "/Unit/SearchUnit",
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

function GetProjectChange(selectObject, dataid, action, Bsns) {
    //if (selectObject.value) {
    //var ItemId = selectObject.value;
    //var prochk = $("#procheck").val();
    //var project = $("#project_name_" + dataid).val();
    //var task = $("#task_name_" + dataid).val();
    //var account = $("#account_name_" + dataid).val();
    //var accjnl = $("#accjnlcheck").val();

    //var flag = true;
    //if (ItemId != null) {
    //    if (project != null) {
    //        var tbody = $("#normalinvoice tbody");
    //        if (tbody.children().length > 2) {
    //            tbody.children("tr").each(function () {
    //                var rowid = $(this).attr("id");
    //                var arr = rowid.split('_');
    //                var arg = arr[1];
    //                var proj = $("#" + rowid + " .project_name").val();
    //                var acc = $("#" + rowid + " .account_name").val();
    //                var tsk = $("#" + rowid + " .task_name").val();

    //                if (dataid != arg && project == proj && task == tsk && account == acc) {
    //                    flag = false;
    //                }
    //            });
    //        }
    //    }
    //    $(selectObject).closest('tr').attr('class', "item_" + ItemId);
    //    if (accjnl == "inactive" && project != null && flag == false) {
    //        alert("You Cannot Add same Account More than 1 Times With Same Project and Task..!!");
    //        $(selectObject).val(null).trigger('change');
    //    } else {
    if ($(".item_").length == 0) {
        addrow('addinvoiceItem', '', '', 0.00, 0.00, '', '', 0.00, Bsns);
    }
    // }

    // }
    // }
    if (Bsns != "Property") {
        searchtask(dataid);
    }
}

function GetTaskChange(selectObject, dataid, action, Bsns) {
    //var flag = true;
    //var ItemId = selectObject.value;
    //var account = $("#account_name_" + dataid).val();
    //var project = $("#project_name_" + dataid).val();
    //var task = $("#task_name_" + dataid).val();
    //var accjnl = $("#accjnlcheck").val();

    //var tbody = $("#normalinvoice tbody");
    //if (tbody.children().length > 2) {
    //    tbody.children("tr").each(function () {
    //        var rowid = $(this).attr("id");
    //        var arr = rowid.split('_');
    //        var arg = arr[1];
    //        var selproj = $("#" + rowid + " .project_name").val();
    //        var seltsk = $("#" + rowid + " .task_name").val();
    //        var acc = $("#" + rowid + " .account_name").val();
    //        if (dataid != arg && account == acc && project == selproj && task == seltsk) {
    //            flag = false;
    //        }
    //    });
    //}
    //$(selectObject).closest('tr').attr('class', "item_" + ItemId);
    //if (accjnl == "inactive" && task != null && flag == false) {
    //    alert("You Cannot Add same Account More than 1 Times With Same Project And Task..!!");
    //    $(selectObject).val(null).trigger('change');
    //} else {
    if ($(".item_").length == 0) {
        addrow('addinvoiceItem', '', '', 0.00, 0.00, '', '', 0.00, Bsns);
    }
    //}
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
    var i = 1;
    $('#addinvoiceItem tr').each(function () {
        $(this).find('td:first').text(i);
        i++;
    });
    rowSubTotal();
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
function journalVSubmitprop(fnval) {
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

    var data = HTMLtbl.getData($('#normalinvoice'));
    var parameters = {};
    parameters.VoucherNo = $('#VoucherNo').val();
    parameters.Date = $('#Date').val();
    parameters.Branch = $('#Branch').val();
    parameters.Paying = $('#total_debit').text();
    parameters.Remark = $('#Remark').val();
    parameters.VATNature = $('#VATNature').val();

    parameters.PDCDate = $('#PDCDate').val();
    parameters.MOPayment = $('#MoPay').val();
    parameters.CheckNo = $('#CheckNo').val();
    parameters.Bank = $('#Bank').val();

    parameters.submittype = fnval;

    parameters.jnlitems = data;

    parameters.Ref1 = $('#Ref1').val();
    parameters.Ref2 = $('#Ref2').val();
    parameters.Ref3 = $('#Ref3').val();
    parameters.Ref4 = $('#Ref4').val();
    parameters.Ref5 = $('#Ref5').val();

    var url = $('#recform')[0].action;

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



            if (e.status) {

                if (e.type == 'print') {
                    printInvoiceJnl(e.data, e.fmapp);
                }
                else {
                    $('.ajax_response', res_success).text(e.message);
                    $('.AlertDiv').prepend(res_success);
                }
                if (url == null) {
                    window.location.href = '/JournalV/Create';
                } else {
                    location.reload();
                }
            }
            else {
                $('.ajax_response', res_danger).text(e.message);
                $('.AlertDiv').prepend(res_danger);
                $("button").prop('disabled', false); // enable button
            }

        }
    });
}
function journalVSubmit(fnval) {
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


    var data = HTMLtbl.getData($('#normalinvoice'));
    var HTMLtbl = {
        getData: function (table) {
            var data = [];
            table.find('tr').not(':first').not('.invoice_').each(function (rowIndex, r) {
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
    var invoicedataref = HTMLtbl.getData($('#invoicedataref'));


    var HTMLtbl = {
        getData: function (table) {
            var data = [];
            table.find('tr').not(':first').not('.invoice_').each(function (rowIndex, r) {
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
    var invoicedataref2 = HTMLtbl.getData($('#invoicedataref2'));


    var HTMLtbl = {
        getData: function (table) {
            var data = [];
            table.find('tr').not(':first').not('.invoice_').each(function (rowIndex, r) {
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
    var invoicedatapay = HTMLtbl.getData($('#invoicedatapay'));

    var parameters = {};
    parameters.VoucherNo = $('#VoucherNo').val();
    parameters.Date = $('#Date').val();
    parameters.invoicedataref = invoicedataref;
    parameters.invoicedataref2 = invoicedataref2;
    parameters.invoicedatapay = invoicedatapay;
    parameters.Branch = $('#Branch').val();
    parameters.Paying = $('#total_debit').text();
    parameters.Remark = $('#Remark').val();
    parameters.VATNature = $('#VATNature').val();

    parameters.PDCDate = $('#PDCDate').val();
    parameters.InvoiceNo = $("#ddlInvoiceNo").val();
    parameters.MOPayment = $('#MoPay').val();
    parameters.CheckNo = $('#CheckNo').val();
    parameters.Bank = $('#Bank').val();
    parameters.PayFrom1 = $('#ddlpayfrom1').val();
    parameters.PayFrom2 = $('#ddlpayfrom2').val();
    parameters.PayTo = $('#ddlpayto').val();
    parameters.submittype = fnval;

    parameters.jnlitems = data;

    parameters.Ref1 = $('#Ref1').val();
    parameters.Ref2 = $('#Ref2').val();
    parameters.Ref3 = $('#Ref3').val();
    parameters.Ref4 = $('#Ref4').val();
    parameters.Ref5 = $('#Ref5').val();

    var url = $('#recform')[0].action;

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
            if (e.status == false) {
                $('.ajax_response', res_danger).text(e.message);
                $('.AlertDiv').prepend(res_danger);
                $("button").prop('disabled', false); // enable button
            }
            else {


                // var fileUpload = $("#RecieptDoc").get(0);
                var fileUpload = $("#input-24").get(0);

                var files = fileUpload.files;

                // Create FormData object
                var fileData = new FormData();

                // Looping over all files and add it to FormData object
                for (var i = 0; i < files.length; i++) {
                    fileData.append(files[i].name, files[i]);
                }

                var Mode = $("#Mode").val();

                //To Get the JournalID in Edit Mode
                var JournalId = $("#JournalId").val();

                // Adding one more key to FormData object
                fileData.append('id', JournalId);

                $.ajax({
                    //url: '/PJournalV/UploadFiles',
                    url: '/JournalV/UploadFiles',
                    type: "POST",
                    contentType: false, // Not to set any content header
                    processData: false, // Not to process data
                    data: fileData,
                    success: function (result) {
                        if (Mode == 'Create')
                            window.location.href = '/JournalV/Create';
                        else
                            window.location.href = '/JournalV/Index';
                    },
                    error: function (err) {
                    }
                });

                if (e.status) {

                    if (e.type == 'print') {
                        printInvoiceJnl(e.data, e.fmapp);
                    }
                    else {
                        $('.ajax_response', res_success).text(e.message);
                        $('.AlertDiv').prepend(res_success);
                    }
                    if (url == null) {
                        // window.location.href = '/JournalV/Create';
                    } else {
                        // location.reload();
                    }
                }
                else {
                    $('.ajax_response', res_danger).text(e.message);
                    $('.AlertDiv').prepend(res_danger);
                    $("button").prop('disabled', false); // enable button
                }
            }

        }
    });
}
function printInvoiceJnl(data, fmap) {
    $("[id$=lblBillNo]").text(data.VoucherNo);
    $("[id$=lblDate]").text(data.Date);

    $("[id$=lblPBy]").text(data.UserName);
    //$("[id$=lblTime]").text(convertToTime(data.Date));

    $("[id$=lbldebitsum]").text(data.Paying.toFixed(2));
    $("[id$=lblcreditsum]").text(data.Paying.toFixed(2));
    if (data.ComHeadCheck == 0) {

        $("#ComHeadCheck").hide();
    }
    else {
        $("#ComHeadCheck").show();
    }
    var prochk = $("#procheck").val();
    $("#lblMOPayment").text(data.MOPay);
    if (data.MOPayment != 0) {
        $(".divpdc").show();
        $("#lblChequeNo").text(data.CheckNo);
        $("#lblBank").text(data.Bank);
    } else {
        $(".divpdc").hide();
    }

    if (data.VATNature == "1") {
        $("#lblVatNature").text("Registered Expense(B2B)");
        $(".divvatnature").show();
    } else {
        $(".divvatnature").hide();
    }

    if (fmap != null) {
        $.each(fmap, function (i, mapp) {

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

    var str = "";
    var count = 1;
    var debit = 0;
    var credit = 0;
    $.each(data.jnlitems, function (i, item) {

        var acctype = item.AccType == 0 ? "Debit" : "Credit";
        var narration = item.Narration != "" ? "<br /><small>" + item.Narration + "</small>" : "";
        var dr = item.Debit > 0 ? parseFloat(item.Debit).toFixed(2) : "";
        var cr = item.Credit > 0 ? parseFloat(item.Credit).toFixed(2) : "";

        str += "<tr class='border-top'>";
        str += '<td>' + count + '</td>';
        str += '<td>' + item.AccountName + narration + '</td>';
        if (prochk == "active") {
            var pro = item.ProjectName != null ? item.ProjectName : "";
            var tsk = item.TaskName != null ? item.TaskName : "";
            str += '<td>' + pro + '</td>';
            str += '<td>' + tsk + '</td>';
        }
        str += '<td class="text-right">' + dr + '</td>';
        str += '<td class="text-right">' + cr + '</td>';

        str += '</tr>';

        count++;
        debit += item.Debit;
        credit += item.Credit;
    });

    var totstr = '<tr style="background: #ccc !important;" class="border-top"><td class="text-left" style="border-right: 0px !important;">(' + (count - 1) + ' Records)</td><td class="text-right" style="border-left: 0px !important;">Total Debit/Credit (AED)</td><td width="20%"  class="text-right">' + parseFloat(debit).toFixed(2) + '</td><td width="20%" class="text-right">' + parseFloat(credit).toFixed(2) + '</td></tr>';

    $('#itemtable1').append(totstr);



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

    $('#itemtable tbody').html("");
    $('#itemtable').append(str);
    var originalpage = document.body.innerHTML;
    var printContent = $('#printit').html();
    $('body').html(printContent);
    window.print();
}



function AccPopup() {
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


    /* function for Edit popup */
    $('#MyTable').on('click', '.modal-edit', function (e) {
        e.preventDefault();
        var url = $(this).attr('href');
        modalshow(url, '#modal-edit');
    });

    $('#modal-change').on('submit', '#createform', function (e) {
        e.preventDefault();
        var url = $('#createform')[0].action;
        var data = $('#createform').serialize();
        createajax(url, data, '#modal-change');
    });

    /* function for gen popup */
    $('span').on('click', '.modal-gen', function (e) {
        e.preventDefault();
        var url = $(this).attr('href');
        modalshow(url, '#modal-gen');
    });


    $('#modal-create').on('shown.bs.modal', function (e) {

        $("#Group").select2({
            placeholder: 'Search Account Group Name ',
            minimumInputLength: 0,
            ajax: {
                url: "/Accounts/SearchInAccGp",
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

    $('#modal-edit').on('shown.bs.modal', function (e) {
        var id = $("#AccountsID").val();
        $("#Group").select2({
            placeholder: 'Search Account Group Name ',
            minimumInputLength: 0,
            ajax: {
                url: "/Accounts/SearchInAccGp",
                dataType: 'json',
                delay: 50,
                data: function (params) {
                    return {
                        q: params.term,
                        y: id,
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
    });


    $("#AccGroup").select2({
        placeholder: 'Search Account Group Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/Accounts/SearchGroup",
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

    $("#ddlAccounts").select2({
        placeholder: 'Search Account by Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/Accounts/SearchAllAccounts",
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

function modechange() {
    var mop = $('#MoPay').val();
    var pdate = $('#PDCDate').val();
    if (mop == 0 || mop == 3) {
        $("#pcdfor").text('');
        $('#PDCDate').val('');
        $('#PDCDate').prop('disabled', true);
        $('#PDCDate').prop('required', false);
        $('#checkno').hide();
        $('#bank').hide();
        $('#PDCDate').attr('tabindex', -1);
    }
    else {
        datepickerInit();
        $("#pcdfor").text('*');
        if (pdate == '') {
            $('#PDCDate').val(today());
        }
        $('#PDCDate').prop('disabled', false);
        $('#PDCDate').prop('required', true);
        $('#checkno').show();
        $('#bank').show();
        var tab = $('#MoPay').attr('tabindex');
        tab++;
        $('#PDCDate').attr('tabindex', tab);
    }
    if ($('#chkPdc').val() == "Yes" && mop == 1) {
        $('#PDCDate').prop('disabled', true);
        $('#PDCDate').val($("#PdcDate").val());
    }
}



function searchInvoice() {
    var selecteditem = new Array();
    $(".invoice_name").each(function () {
        selecteditem.push($(this).val());
    });

    var account = $("#ddlpayfrom1").val();

    $(".invoice_name").select2({
        placeholder: 'Search Invoice',
        minimumInputLength: 0,
        ajax: {
            url: "/Accounts/SearchAccountsByIdSelect",
            dataType: 'json',
            type: "POST",
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 1,
                    account: account
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
    var markup = '<div class="se-row">' +
        '<h4>' + repo.text + '</h4>';
    if (repo.Date != null) {
        markup += '<div class="se-sec">  Date  : ' + convertToDate(repo.Date) + '</div>';
    }

    markup += '<div class="se-sec">Amount  : ' + repo.Amount + '</div>';
    markup += '<div class="se-sec">Balance : ' + repo.Balance + '</div>';
    markup += '</div>';
    var retn = $(markup);
    return retn;
}

function repoFormatSelection(repo) {
    return repo.text;
}

function GetTypeChange(selectObject, dataid, action) {

    //if (action == "new") {
    if (selectObject.value == "New Reference") {

        $(".td_invoice_" + dataid).hide();
        $(".td_refname_" + dataid).show();

        $("#td_invoice_" + dataid).hide();
        $("#td_refname_" + dataid).show();

        $("#newrefname_" + dataid).prop('disabled', false);

    } else if (selectObject.value == "Against Reference") {


        $(".td_invoice_" + dataid).show();
        $(".td_refname_" + dataid).hide();

        $("#td_invoice_" + dataid).show();
        $("#td_refname_" + dataid).hide();

        //$("#newrefname_" + dataid).prop('disabled', false);


    } else if (selectObject.value == "Advance") {


        $(".td_invoice_" + dataid).hide();
        $(".td_refname_" + dataid).hide();

        $("#td_invoice_" + dataid).hide();
        $("#td_refname_" + dataid).show();

        $("#newrefname_" + dataid).prop('disabled', true);
    }
    else if (selectObject.value == "On Account") {

        $(".td_invoice_" + dataid).hide();
        $(".td_refname_" + dataid).hide();

        $("#td_invoice_" + dataid).hide();
        $("#td_refname_" + dataid).show();

        $("#newrefname_" + dataid).prop('disabled', true);

    }
    //}
}
function GetTypeChange2(selectObject, dataid, action) {

    //if (action == "new") {
    if (selectObject.value == "New Reference") {

        $(".td_invoice2_" + dataid).hide();
        $(".td_refname2_" + dataid).show();

        $("#td_invoice2_" + dataid).hide();
        $("#td_refname2_" + dataid).show();

        $("#newrefname2_" + dataid).prop('disabled', false);

    } else if (selectObject.value == "Against Reference") {


        $(".td_invoice2_" + dataid).show();
        $(".td_refname2_" + dataid).hide();

        $("#td_invoice2_" + dataid).show();
        $("#td_refname2_" + dataid).hide();

        //$("#newrefname_" + dataid).prop('disabled', false);


    } else if (selectObject.value == "Advance") {


        $(".td_invoice2_" + dataid).hide();
        $(".td_refname2_" + dataid).hide();

        $("#td_invoice2_" + dataid).hide();
        $("#td_refname2_" + dataid).show();

        $("#newrefname2_" + dataid).prop('disabled', true);
    }
    else if (selectObject.value == "On Account") {

        $(".td_invoice2_" + dataid).hide();
        $(".td_refname2_" + dataid).hide();

        $("#td_invoice2_" + dataid).hide();
        $("#td_refname2_" + dataid).show();

        $("#newrefname2_" + dataid).prop('disabled', true);

    }
    //}
}
function initialload() {
    $("#td_invoice").hide();
    $("#td_refname").show();

    $("#td_headnull").hide();
}
function GetInvoiceDetails(selectObject, dataid, action) {
    if (selectObject.value) {
        var ItemId = selectObject.value;
        if (ItemId != null) {
            if ($(".invoice_" + ItemId).length > 0) {
                alert("Sorry You Cant Add An Item More Than One Time");
                $(selectObject).val(null).trigger('change');
            }
            else {
                itemUpdate(selectObject, dataid, action);
            }
        }
    }
}

function itemUpdate(selectObject, dataid, action) {
    var entry = "";
    var url = "";
    var payfrom = $("#ddlpayfrom1").val();
    //if (action == "edit") {
    //    url = '/Receipt/GetReceiptBill';
    //    sentry = getQueryString('');
    //} else {
    url = '/Accounts/SearchAccountsById';
    entry = selectObject.value;
    //}

    $.ajax({
        url: url,
        dataType: 'json',
        data: { account: payfrom, entry: entry },
        cache: true,
        success: function (data) {

            $("#newrefname_" + dataid).val(data.text);


            $("#invoice_balance_" + dataid).val(data.Balance);
            if (data.Date != null) {
                $("#invoice_date_" + dataid).val(convertToDate(data.Date));
            }
            $("#invoice_balance_" + dataid).val(parseFloat(data.Balance).toFixed(2));

            //var amt = data.Balance < data.Amount ? data.Balance : data.Amount;
            if (action == "edit") {
                $("#invoice_amt_" + dataid).val(parseFloat(data.Amount).toFixed(2));
            } else {
                $("#invoice_amt_" + dataid).val(parseFloat(data.Balance).toFixed(2));
            }

            if (data.Amount > 0) {
                $("#Paying").prop('disabled', true);
            } else {
                $("#Paying").prop('disabled', false);
            }

            //$("#invoice_amt_" + dataid).val(parseFloat(data.Amount).toFixed(2));

            $("#invoice_type_" + dataid).val(data.type);
            // $("#invoice_balanceamt_" + dataid).val(data.BalAmt);

            $(selectObject).closest('tr').attr('class', "invoice_" + data.id);

            if (data.id != null && data.Amount > 0) {
                var count = 0;
                $("#addinvoiceItem tr").each(function () {
                    var classname = $(this).closest('tr').attr('class');
                    if (classname == 'invoice_') {
                        count++;
                    }
                });
                if (count == 0)
                    addrowsref('addinvoiceItemref', 'new', '', '', '', '0.00', '0.00', '', '');
            }
            RowTotal();
        }
    });
}

function invoice_amt_change(arg) {
    var bal = $("#invoice_balance_" + arg).val();
    var amt = $("#invoice_amt_" + arg).val();
    var type = $("#type_name_" + arg).val();

    //if (type == "Against Reference") {
    //    if (bal != 0 && parseFloat(bal) < parseFloat(amt)) {
    //        alert("Amount Should Less than or Equals to Balance Amount..!!");
    //        $("#invoice_amt_" + arg).val(parseFloat(bal).toFixed(2));
    //    }
    //}

    if (parseFloat(amt) > 0) {
        $("#Paying").prop('disabled', true);
    } else {
        $("#Paying").prop('disabled', false);
    }
    $("#invoice_amt_" + arg).closest('tr').attr('class', "invoice_" + arg);


    //--------check empty rows----------------------------------------
    var count = 0;
    $("#addinvoiceItem tr").each(function () {
        var classname = $(this).closest('tr').attr('class');
        if (classname == 'invoice_') {
            count++;
        }
    });

    if (count == 0)
        addrowsref('addinvoiceItemref', 'new', '', '', '', '0.00', '0.00', '', '');
    //-------------------------------------------------------------------

    initialload();


    RowTotal();
    cashBalance();
}

//Delete a row of table
function deleteRow(t, item) {
    var classname = $(t).closest('tr').attr('class');
    if (classname == 'invoice_') alert("Sorry you can't delete this row.");
    else {
        var r = confirm("Are you sure you want to delete this..?");
        if (r == true) {
            var e = t.parentNode.parentNode;
            e.parentNode.removeChild(e);
        }
    }
    rowSubTotal();
    var i = 1;
    $('#addinvoiceItem tr').each(function () {
        $(this).find('td:first').text(i);
        i++;
    });
    chkAmount();
    cashBalance();
}
function RowTotal() {
    var tbody = $("#invoicedataref tbody");
    if (tbody.children().length >= 0) {
        var totAmt = 0;
        $(".invamt").each(function () {
            var Amt = this.value;
            Amt = Amt || 0;
            totAmt = parseFloat(totAmt) + parseFloat(Amt);
        });
        $("#GrandTotal").val(totAmt.toFixed(2));
        $("#Paying").val(totAmt.toFixed(2));
        //$("#Discount").val(0.00);
    }
}








function GetInvoiceDetails2(selectObject, dataid, action) {
    if (selectObject.value) {
        var ItemId = selectObject.value;
        if (ItemId != null) {
            if ($(".invoice_" + ItemId).length > 0) {
                alert("Sorry You Cant Add An Item More Than One Time");
                $(selectObject).val(null).trigger('change');
            }
            else {
                itemUpdate2(selectObject, dataid, action);
            }
        }
    }
}

function itemUpdate2(selectObject, dataid, action) {
    var entry = "";
    var url = "";
    var payfrom = $("#ddlpayfrom2").val();
    //if (action == "edit") {
    //    url = '/Receipt/GetReceiptBill';
    //    sentry = getQueryString('');
    //} else {
    url = '/Accounts/SearchAccountsById';
    entry = selectObject.value;
    //}

    $.ajax({
        url: url,
        dataType: 'json',
        data: { account: payfrom, entry: entry },
        cache: true,
        success: function (data) {

            $("#newrefname2_" + dataid).val(data.text);


            $("#invoice_balance_" + dataid).val(data.Balance);
            if (data.Date != null) {
                $("#invoice_date_" + dataid).val(convertToDate(data.Date));
            }
            $("#invoice_balance_" + dataid).val(parseFloat(data.Balance).toFixed(2));

            //var amt = data.Balance < data.Amount ? data.Balance : data.Amount;
            if (action == "edit") {
                $("#invoice_amt2_" + dataid).val(parseFloat(data.Amount).toFixed(2));
            } else {
                $("#invoice_amt2_" + dataid).val(parseFloat(data.Balance).toFixed(2));
            }

            if (data.Amount > 0) {
                $("#Paying").prop('disabled', true);
            } else {
                $("#Paying").prop('disabled', false);
            }

            //$("#invoice_amt_" + dataid).val(parseFloat(data.Amount).toFixed(2));

            $("#invoice_type_" + dataid).val(data.type);
            // $("#invoice_balanceamt_" + dataid).val(data.BalAmt);

            $(selectObject).closest('tr').attr('class', "invoice_" + data.id);

            if (data.id != null && data.Amount > 0) {
                var count = 0;
                $("#addinvoiceItem tr").each(function () {
                    var classname = $(this).closest('tr').attr('class');
                    if (classname == 'invoice_') {
                        count++;
                    }
                });
                if (count == 0)
                    addrowsref2('addinvoiceItemref2', 'new', '', '', '', '0.00', '0.00', '', '');
            }
            RowTotal2();
        }
    });
}

function invoice_amt_change2(arg) {
    var bal = $("#invoice_balance_" + arg).val();
    var amt = $("#invoice_amt_" + arg).val();
    var type = $("#type_name_" + arg).val();

    ////if (type == "Against Reference") {
    ////    if (bal != 0 && parseFloat(bal) < parseFloat(amt)) {
    ////        alert("Amount Should Less than or Equals to Balance Amount..!!");
    ////        $("#invoice_amt_" + arg).val(parseFloat(bal).toFixed(2));
    ////    }
    ////}

    if (parseFloat(amt) > 0) {
        $("#Paying").prop('disabled', true);
    } else {
        $("#Paying").prop('disabled', false);
    }
    $("#invoice_amt_" + arg).closest('tr').attr('class', "invoice_" + arg);


    //--------check empty rows----------------------------------------
    var count = 0;
    $("#addinvoiceItem2 tr").each(function () {
        var classname = $(this).closest('tr').attr('class');
        if (classname == 'invoice_') {
            count++;
        }
    });

    if (count == 0)
        addrowsref2('addinvoiceItemref2', 'new', '', '', '', '0.00', '0.00', '', '');
    //-------------------------------------------------------------------

    initialload();


    RowTotal();
    cashBalance();
}

//Delete a row of table
function deleteRow2(t, item) {
    var classname = $(t).closest('tr').attr('class');
    if (classname == 'invoice_') alert("Sorry you can't delete this row.");
    else {
        var r = confirm("Are you sure you want to delete this..?");
        if (r == true) {
            var e = t.parentNode.parentNode;
            e.parentNode.removeChild(e);
        }
    }
    RowTotal2();
    var i = 1;
    $('#addinvoiceItem tr').each(function () {
        $(this).find('td:first').text(i);
        i++;
    });
    chkAmount();
    cashBalance();
}
function RowTotal2() {
    var tbody = $("#invoicedataref2 tbody");
    if (tbody.children().length > 0) {
        var totAmt = 0;
        $(".invamt").each(function () {
            var Amt = this.value;
            Amt = Amt || 0;
            totAmt = parseFloat(totAmt) + parseFloat(Amt);
        });
        $("#GrandTotal").val(totAmt.toFixed(2));
        $("#Paying").val(totAmt.toFixed(2));
        //$("#Discount").val(0.00);
    }
}












function PaymentFrom() {

    // paid to atribute
    $("#ddlpayfrom1").select2({
        placeholder: 'Search Account By Code or name',
        minimumInputLength: 0,
        ajax: {
            url: "/Accounts/SearchAccounts",
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
        templateResult: SelectToGroup,
        templateSelection: ToSetFormatSelection,
    });
    $("#ddlpayfrom2").select2({
        placeholder: 'Search Account By Code or name',
        minimumInputLength: 0,
        ajax: {
            url: "/Accounts/SearchAccounts",
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
        templateResult: SelectToGroup,
        templateSelection: ToSetFormatSelection,
    });
}

function accbalance(accid, receiptchk) {
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
            if (data.data != null) {
                //bindinvoice(data.data, receiptchk);
                bindinvoiceNew();

                $('.exp').hide();
                $('.sup').show();
            }
            else {
                $('#invoice').html('<hr/>');
                $('.exp').show();
                $('.sup').hide();
                grandTotal();
            }
            cashBalance();
        }
    });
}
function bindinvoiceNew() {
    var table = '<table class="table table-bordered table-hover text-center" id="invoicedataref">' +
        '<thead>' +
        '<tr class="bg-gray">' +
        '<th class="text-center">#</th><th class="text-center">Type of Ref</th>' +
        '<th id="headinvoice" class="text-center">Invoice No / Name</th>' +
        //'<th class="text-center">Date</th><th class="text-center">Balance</th>' +
        '<th class="text-center">Amount</th>' +
        '<th class="text-center">Action</th>' +
        '</tr>' +
        '</thead>' +
        '<tbody id="addinvoiceItemref"></tbody></table>';
    $('#invoice').html(table);

    addrowsref('addinvoiceItemref', 'new', '', '', '', '0.00', '0.00', '', '');
}
var countref1 = 1, type = '';
limits = 500; var Amt = 0;
function addrowsref(t, action, Invoice, BillNo, InvoiceDate, Balance, Amount, Type, NewRefName, RType) {

    tabindex = countref1 * 5;
    var slno = $('#addinvoiceItemref tr').length + 1;
    var row = "<tr class='invoice_' id='invoice_" + countref1 + "'>";
    var divid = "invoice_name_" + Invoice;
    tab1 = tabindex + 1;
    tab2 = tabindex + 2;
    tab3 = tabindex + 3;
    tab4 = tabindex + 4;
    tab5 = tabindex + 5;

    var hide = "";
    if (action == "new") {
        initialload();
        hide = "hidden";
    }

    var rowOne = "";
    var rowTwo = "";
    var rowread = "";
    var row1 = "";
    var row2 = "";
    var row3 = "";
    var row4 = "";

    if (RType != null) {
        row = "<tr class='invoice_" + countref1 + "' id='invoice_" + countref1 + "'>";
    }

    if (action == "edit") {
        if (RType == "New Reference") {
            rowOne = "hidden";
            row1 = "Selected";
        } else if (RType == "Against Reference") {
            rowTwo = "hidden";
            row2 = "Selected";
        } else if (RType == "Advance") {
            rowOne = "hidden";
            rowread = "readonly='readonly'";
            NewRefName = "";
            row3 = "Selected";
        } else if (RType == "On Account") {
            rowOne = "hidden";
            rowread = "readonly='readonly'";
            NewRefName = "";
            row4 = "Selected";
        } else {
            rowOne = "hidden";
            NewRefName = "";
            row1 = "Selected";
        }
    }

    var Option = "";
    if (Invoice != null) {
        Option = "<option value='" + Invoice + "'>" + BillNo + "</option>";
    }

    var OptionType = "<option value='New Reference' " + row1 + ">New Reference</option>" +
        "<option value='Against Reference' " + row2 + ">Against Reference</option>" +
        "<option value='Advance' " + row3 + ">Advance</option>" +
        "<option value='On Account' " + row4 + ">On Account</option>";

    data = "<td class='text-center' id=" + divid + "> " + slno + " </td>" +
        "<td id='td_type_" + countref1 + "' class='input-group input-group-sm td_type' width='100%'><select data-name='Type' class='form-control type_name' data-id='" + countref1 + "' placeholder='Select Type' id='type_name_" + countref1 + "' onchange='GetTypeChange(this," + countref1 + ",\"" + action + "\")'>" + OptionType + "</select></td>" +
        "<td " + hide + rowOne + " id='td_invoice_" + countref1 + "' class='td_invoice'><select data-name='InvoiceNo' class='form-control invoice_name' data-id='" + countref1 + "' placeholder='Select Invoice' id='invoice_name_" + countref1 + "' onchange='GetInvoiceDetails(this," + countref1 + ",\"" + action + "\")'>" + Option + "</select></td>" +
        //"<td><input type='text' data-name='' id='invoice_date_" + countref1 + "' value='" + InvoiceDate + "'  class='invoice_date_" + countref1 + " form-control text-center' tabindex='" + tab2 + "' readonly='readonly' /></td>" +
        "<td " + rowTwo + " id='td_refname_" + countref1 + "' class='td_refname'><input type='text' " + rowread + " data-name='NewRefName' id='newrefname_" + countref1 + "' value='" + NewRefName + "'  class='newrefname_" + countref1 + " form-control text-center' tabindex='" + tab2 + "' /></td>" +
        "<td><input type='number' data-name='Amount' onchange='invoice_amt_change(" + countref1 + ");' id='invoice_amt_" + countref1 + "' value='" + parseFloat(Amount).toFixed(2) + "'  class='invoice_amt_" + countref1 + " form-control text-center invamt' placeholder='0' min='0' tabindex='" + tab4 + "' /></td>" +
        "<td><button type='button' tabindex='" + tab5 + "' style='text-align: right;' class='btn btn-danger'  value='Delete' onclick='deleteRow(this)'><i class='fa fa-trash fa-1x'></i></button> " +
        "<input type='hidden' data-name='BillType'  class='invoice_type_" + countref1 + "' id='invoice_type_" + countref1 + "' value='" + Type + "'/>" +
        "<input type='hidden' data-name='' id='invoice_balance_" + countref1 + "' value='" + parseFloat(Balance).toFixed(2) + "'  class='invoice_balance_" + countref1 + " ' />" +
        //"<input type='hidden' data-name='' class='invoice_balanceamt_" + countref1 + "' id='invoice_balanceamt_" + countref1 + "' value='" + BalAmt + "'/>"+
        "</td>";
    row += data + "</tr>";
    $('#' + t).append(row);
    searchInvoice();

    countref1++;
    setTabIndex();

    Amt += parseFloat(Amount);
    if (Amt > 0) {
        $("#Paying").prop('disabled', true);
    }



}
function searchInvoice() {
    var selecteditem = new Array();
    $(".invoice_name").each(function () {
        selecteditem.push($(this).val());
    });

    var account = $("#ddlpayfrom1").val();

    $(".invoice_name").select2({
        placeholder: 'Search Invoice',
        minimumInputLength: 0,
        ajax: {
            url: "/Accounts/SearchAccountsByIdSelect",
            dataType: 'json',
            type: "POST",
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 1,
                    account: account
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



function accbalance2(accid, receiptchk) {
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
            $('#accdetails2').text(amount);
            $('#accbalance').show();
            if (data.data != null) {
                //bindinvoice(data.data, receiptchk);
                bindinvoiceNew2();

                $('.exp').hide();
                $('.sup').show();
            }
            else {
                $('#invoice').html('<hr/>');
                $('.exp').show();
                $('.sup').hide();
                grandTotal();
            }
            cashBalance();
        }
    });
}
function bindinvoiceNew2() {
    var table = '<table class="table table-bordered table-hover text-center" id="invoicedataref2">' +
        '<thead>' +
        '<tr class="bg-gray">' +
        '<th class="text-center">#</th><th class="text-center">Type of Ref</th>' +
        '<th id="headinvoice" class="text-center">Invoice No / Name</th>' +
        //'<th class="text-center">Date</th><th class="text-center">Balance</th>' +
        '<th class="text-center">Amount</th>' +
        '<th class="text-center">Action</th>' +
        '</tr>' +
        '</thead>' +
        '<tbody id="addinvoiceItemref2"></tbody></table>';
    $('#invoice2').html(table);

    addrowsref2('addinvoiceItemref2', 'new', '', '', '', '0.00', '0.00', '', '');
}
var countref2 = 1, type = '';
limits = 500; var Amt = 0;
function addrowsref2(t, action, Invoice, BillNo, InvoiceDate, Balance, Amount, Type, NewRefName, RType) {

    tabindex = countref2 * 5;
    var slno = $('#addinvoiceItemref2 tr').length + 1;
    var row = "<tr class='invoice_' id='invoice_" + countref2 + "'>";
    var divid = "invoice_name_" + Invoice;
    tab1 = tabindex + 1;
    tab2 = tabindex + 2;
    tab3 = tabindex + 3;
    tab4 = tabindex + 4;
    tab5 = tabindex + 5;

    var hide = "";
    if (action == "new") {
        initialload();
        hide = "hidden";
    }

    var rowOne = "";
    var rowTwo = "";
    var rowread = "";
    var row1 = "";
    var row2 = "";
    var row3 = "";
    var row4 = "";

    if (RType != null) {
        row = "<tr class='invoice_" + countref2 + "' id='invoice_" + countref2 + "'>";
    }

    if (action == "edit") {
        if (RType == "New Reference") {
            rowOne = "hidden";
            row1 = "Selected";
        } else if (RType == "Against Reference") {
            rowTwo = "hidden";
            row2 = "Selected";
        } else if (RType == "Advance") {
            rowOne = "hidden";
            rowread = "readonly='readonly'";
            NewRefName = "";
            row3 = "Selected";
        } else if (RType == "On Account") {
            rowOne = "hidden";
            rowread = "readonly='readonly'";
            NewRefName = "";
            row4 = "Selected";
        } else {
            rowOne = "hidden";
            NewRefName = "";
            row1 = "Selected";
        }
    }

    var Option = "";
    if (Invoice != null) {
        Option = "<option value='" + Invoice + "'>" + BillNo + "</option>";
    }

    var OptionType = "<option value='New Reference' " + row1 + ">New Reference</option>" +
        "<option value='Against Reference' " + row2 + ">Against Reference</option>" +
        "<option value='Advance' " + row3 + ">Advance</option>" +
        "<option value='On Account' " + row4 + ">On Account</option>";

    data = "<td class='text-center' id=" + divid + "> " + slno + " </td>" +
        "<td id='td_type2_" + countref2 + "' class='input-group input-group-sm td_type2' width='100%'><select data-name='Type' class='form-control type_name' data-id='" + countref2 + "' placeholder='Select Type' id='type_name_" + countref2 + "' onchange='GetTypeChange2(this," + countref2 + ",\"" + action + "\")'>" + OptionType + "</select></td>" +
        "<td " + hide + rowOne + " id='td_invoice2_" + countref2 + "' class='td_invoice2'><select data-name='InvoiceNo' class='form-control invoice_name2' data-id='" + countref2 + "' placeholder='Select Invoice' id='invoice_name_" + countref2 + "' onchange='GetInvoiceDetails2(this," + countref2 + ",\"" + action + "\")'>" + Option + "</select></td>" +
        //"<td><input type='text' data-name='' id='invoice_date_" + countref2 + "' value='" + InvoiceDate + "'  class='invoice_date_" + countref2 + " form-control text-center' tabindex='" + tab2 + "' readonly='readonly' /></td>" +
        "<td " + rowTwo + " id='td_refname2_" + countref2 + "' class='td_refname2'><input type='text' " + rowread + " data-name='NewRefName' id='newrefname2_" + countref2 + "' value='" + NewRefName + "'  class='newrefname2_" + countref2 + " form-control text-center' tabindex='" + tab2 + "' /></td>" +
        "<td><input type='number' data-name='Amount' onchange='invoice_amt_change2(" + countref2 + ");' id='invoice_amt2_" + countref2 + "' value='" + parseFloat(Amount).toFixed(2) + "'  class='invoice_amt_" + countref2 + " form-control text-center invamt' placeholder='0' min='0' tabindex='" + tab4 + "' /></td>" +
        "<td><button type='button' tabindex='" + tab5 + "' style='text-align: right;' class='btn btn-danger'  value='Delete' onclick='deleteRow(this)'><i class='fa fa-trash fa-1x'></i></button> " +
        "<input type='hidden' data-name='BillType'  class='invoice_type_" + countref2 + "' id='invoice_type_" + countref2 + "' value='" + Type + "'/>" +
        "<input type='hidden' data-name='' id='invoice_balance_" + countref2 + "' value='" + parseFloat(Balance).toFixed(2) + "'  class='invoice_balance_" + countref2 + " ' />" +
        //"<input type='hidden' data-name='' class='invoice_balanceamt_" + countref2 + "' id='invoice_balanceamt_" + countref2 + "' value='" + BalAmt + "'/>"+
        "</td>";
    row += data + "</tr>";
    $('#' + t).append(row);
    searchInvoice2();

    countref2++;
    setTabIndex();

    Amt += parseFloat(Amount);
    if (Amt > 0) {
        $("#Paying").prop('disabled', true);
    }



}
function searchInvoice2() {
    var selecteditem = new Array();
    $(".invoice_name").each(function () {
        selecteditem.push($(this).val());
    });

    var account = $("#ddlpayfrom2").val();

    $(".invoice_name2").select2({
        placeholder: 'Search Invoice',
        minimumInputLength: 0,
        ajax: {
            url: "/Accounts/SearchAccountsByIdSelect",
            dataType: 'json',
            type: "POST",
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 1,
                    account: account
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
    var markup = '<div class="se-row">' +
        '<h4>' + repo.text + '</h4>';
    if (repo.Date != null) {
        markup += '<div class="se-sec">  Date  : ' + convertToDate(repo.Date) + '</div>';
    }

    markup += '<div class="se-sec">Amount  : ' + repo.Amount + '</div>';
    markup += '<div class="se-sec">Balance : ' + repo.Balance + '</div>';
    markup += '</div>';
    var retn = $(markup);
    return retn;
}

function repoFormatSelection(repo) {
    return repo.text;
}
function chkacctype(accid) {
    $.ajax({
        async: true,
        cache: false,
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/Accounts/chkAccountType",
        data: JSON.stringify({ account: accid }),
        success: function (data) {
            if (data == "Supplier") {
                $("#divproject").hide();
            } else if (data == "Customer") {
                $("#divproject").show();

                $('#ddlProject').val(null).trigger('change');
                $('#ddlProTask').val(null).trigger('change');
                CallProjectByAcc();
                CallTask();
            }
            else {
                $("#divproject").show();
                CallProjectAll();
                CallTask();
            }
        }
    });
}
function chkacctype2(accid) {
    $.ajax({
        async: true,
        cache: false,
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/Accounts/chkAccountType",
        data: JSON.stringify({ account: accid }),
        success: function (data) {
            if (data == "Supplier") {
                $("#divproject").hide();
            } else if (data == "Customer") {
                $("#divproject").show();

                $('#ddlProject').val(null).trigger('change');
                $('#ddlProTask').val(null).trigger('change');
                CallProjectByAcc();
                CallTask();
            }
            else {
                $("#divproject").show();
                CallProjectAll();
                CallTask();
            }
        }
    });
}

function cashBalance() {
    var totAmt = 0;
    var tbody = $("#invoicedata tbody");
    if (tbody.children().length > 0) {
        $(".invamt").each(function () {
            var Amt = this.value;
            Amt = Amt || 0;
            totAmt = parseFloat(totAmt) + parseFloat(Amt);
        });
    }
    if (totAmt > 0) {

        var totalPayable = $("#GrandTotal").val();
        var totalPaid = totAmt;
        var discount = parseFloat($("#Discount").val());

        //alert(totalPayable + totalPaid + discount);
        totalPayable = totalPayable || 0;
        totalPaid = totalPaid || 0;
        discount = discount || 0;


        var tbody = $("#invoicedata tbody");
        if (tbody.children().length == 1) {
            var payaAmt = totalPaid - discount;
            $("#GrandTotal").val(payaAmt.toFixed(2));
        } else {
            var pay = parseFloat(totalPaid) - parseFloat(discount);
            var balance = parseFloat(totalPaid) - ((parseFloat(totalPaid) - parseFloat(discount)) + parseFloat(discount));
            var GT = parseFloat(totalPaid); //+ parseFloat(discount);
            $("#Paying").val(pay.toFixed(2));
            $("#GrandTotal").val(GT.toFixed(2));
            $("#Balance").val(balance.toFixed(2));


            $("#Discount").val(discount.toFixed(2));
        }
    }
}
function chkacctype3(accid) {
    $.ajax({
        async: true,
        cache: false,
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/Accounts/chkAccountType",
        data: JSON.stringify({ account: accid }),
        success: function (data) {
            if (data == "Supplier") {
                $("#divproject").hide();
            } else if (data == "Customer") {
                $("#divproject").show();
                CallProjectAll();
                CallTask();
            }
            else {
                $("#divproject").show();
                CallProjectAll();
                CallTask();
            }
        }
    });
}
function accbalancepay(accid, chkPayment) {
    $.ajax({
        async: true,
        cache: false,
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/Accounts/ChekPurchase",
        data: JSON.stringify({ id: accid }),
        success: function (data) {
            var amount = data.balance.amount.toFixed(2) + " " + data.balance.type;
            $('#accdetails').text(amount);
            $('#accbalance').show();

            if (data.data != null) {
                //bindinvoice(data.data, chkPayment);
                bindinvoiceNewpay();

                $('.exp').hide();
                $('.sup').show();
            }
            else {
                $('#invoice').html('<hr/>');
                $('.exp').show();
                $('.sup').hide();
                grandTotal();

            }
            cashBalance();
        }
    });
}
function bindinvoiceNewpay() {
    var table = '<table class="table table-bordered table-hover text-center" id="invoicedatapay">' +
        '<thead>' +
        '<tr class="bg-gray">' +
        '<th class="text-center">#</th><th class="text-center">Type of Ref</th>' +
        '<th id="headinvoice" class="text-center">Invoice No / Name</th>' +
        //'<th class="text-center">Date</th><th class="text-center">Balance</th>' +
        '<th class="text-center">Amount</th>' +
        '<th class="text-center">Action</th>' +
        '</tr>' +
        '</thead>' +
        '<tbody id="addinvoiceItempay"></tbody></table>';
    $('#invoice3').html(table);

    addrowspay('addinvoiceItempay', 'new', '', '', '', '0.00', '0.00', '', '');
}
var paycount = 1, type = '';
limits = 500; var Amt = 0;
function addrowspay(t, action, Invoice, BillNo, InvoiceDate, Balance, Amount, Type, NewRefName, RType) {

    tabindex = paycount * 5;
    var slno = $('#addinvoiceItempay tr').length + 1;
    var row = "<tr class='invoice_' id='invoice_" + paycount + "'>";
    var divid = "invoice_name_" + Invoice;
    tab1 = tabindex + 1;
    tab2 = tabindex + 2;
    tab3 = tabindex + 3;
    tab4 = tabindex + 4;
    tab5 = tabindex + 5;

    var hide = "";
    if (action == "new") {
        initialload();
        hide = "hidden";
    }

    var rowOne = "";
    var rowTwo = "";
    var rowread = "";
    var row1 = "";
    var row2 = "";
    var row3 = "";
    var row4 = "";

    if (RType != null) {
        row = "<tr class='invoice_" + paycount + "' id='invoice_" + paycount + "'>";
    }

    if (action == "edit") {
        if (RType == "New Reference") {
            rowOne = "hidden";
            row1 = "Selected";
        } else if (RType == "Against Reference") {
            rowTwo = "hidden";
            row2 = "Selected";
        } else if (RType == "Advance") {
            rowOne = "hidden";
            rowread = "readonly='readonly'";
            NewRefName = "";
            row3 = "Selected";
        } else if (RType == "On Account") {
            rowOne = "hidden";
            rowread = "readonly='readonly'";
            NewRefName = "";
            row4 = "Selected";
        } else {
            rowOne = "hidden";
            NewRefName = "";
            row1 = "Selected";
        }
    }

    var Option = "";
    if (Invoice != null) {
        Option = "<option value='" + Invoice + "'>" + BillNo + "</option>";
    }

    var OptionType = "<option value='New Reference' " + row1 + ">New Reference</option>" +
        "<option value='Against Reference' " + row2 + ">Against Reference</option>" +
        "<option value='Advance' " + row3 + ">Advance</option>" +
        "<option value='On Account' " + row4 + ">On Account</option>";

    data = "<td class='text-center' id=" + divid + "> " + slno + " </td>" +
        "<td id='td_type_" + paycount + "' class='input-group input-group-sm td_type' width='100%'><select data-name='Type' class='form-control type_name' data-id='" + paycount + "' placeholder='Select Type' id='type_name_" + paycount + "' onchange='GetTypeChange(this," + paycount + ",\"" + action + "\")'>" + OptionType + "</select></td>" +
        "<td " + hide + rowOne + " id='td_invoice_" + paycount + "' class='td_invoice'><select data-name='InvoiceNo' class='form-control invoice_name' data-id='" + paycount + "' placeholder='Select Invoice' id='invoice_name_" + paycount + "' onchange='GetInvoiceDetailspay(this," + paycount + ",\"" + action + "\")'>" + Option + "</select></td>" +
        //"<td><input type='text' data-name='' id='invoice_date_" + paycount + "' value='" + InvoiceDate + "'  class='invoice_date_" + paycount + " form-control text-center' tabindex='" + tab2 + "' readonly='readonly' /></td>" +
        "<td " + rowTwo + " id='td_refname_" + paycount + "' class='td_refname'><input type='text' " + rowread + " data-name='NewRefName' id='newrefname_" + paycount + "' value='" + NewRefName + "'  class='newrefname_" + paycount + " form-control text-center' tabindex='" + tab2 + "' /></td>" +
        "<td><input type='number' data-name='Amount' onchange='invoice_amt_change(" + paycount + ");' id='invoice_amt_" + paycount + "' value='" + parseFloat(Amount).toFixed(2) + "'  class='invoice_amt_" + paycount + " form-control text-center invamt' placeholder='0' min='0' tabindex='" + tab4 + "' /></td>" +
        "<td><button type='button' tabindex='" + tab5 + "' style='text-align: right;' class='btn btn-danger'  value='Delete' onclick='deleteRow(this)'><i class='fa fa-trash fa-1x'></i></button> " +
        "<input type='hidden' data-name='BillType'  class='invoice_type_" + paycount + "' id='invoice_type_" + paycount + "' value='" + Type + "'/>" +
        "<input type='hidden' data-name='' id='invoice_balance_" + paycount + "' value='" + parseFloat(Balance).toFixed(2) + "'  class='invoice_balance_" + paycount + " ' />" +
        //"<input type='hidden' data-name='' class='invoice_balanceamt_" + paycount + "' id='invoice_balanceamt_" + paycount + "' value='" + BalAmt + "'/>"+
        "</td>";
    row += data + "</tr>";
    $('#' + t).append(row);
    searchInvoicepay();

    paycount++;
    setTabIndex();

    Amt += parseFloat(Amount);
    if (Amt > 0) {
        $("#Paying").prop('disabled', true);
    }
    (Amt = 0) ? $('#LblDiscount').hide() : $('#LblDiscount').show();
}
function searchInvoicepay() {
    var selecteditem = new Array();
    $(".invoice_name").each(function () {
        selecteditem.push($(this).val());
    });

    var account = $("#ddlpayto").val();

    $(".invoice_name").select2({
        placeholder: 'Search Invoice',
        minimumInputLength: 0,
        ajax: {
            url: "/Accounts/SearchAccountsByIdpaySelect",
            dataType: 'json',
            type: "POST",
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 1,
                    account: account
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
        templateResult: repoFormatResultpay,
        templateSelection: repoFormatSelectionpay,
    });
}

function repoFormatResultpay(repo) {
    var markup = '<div class="se-row">' +
        '<h4>' + repo.text + '</h4>';
    if (repo.Date != null) {
        markup += '<div class="se-sec">  Date  : ' + convertToDate(repo.Date) + '</div>';
    }

    markup += '<div class="se-sec">Amount  : ' + repo.Amount + '</div>';
    markup += '<div class="se-sec">Balance : ' + repo.Balance + '</div>';
    markup += '</div>';
    var retn = $(markup);
    return retn;
}

function repoFormatSelectionpay(repo) {
    return repo.text;
}
function GetInvoiceDetailspay(selectObject, dataid, action) {
    if (selectObject.value) {
        var ItemId = selectObject.value;
        if (ItemId != null) {
            if ($(".invoice_" + ItemId).length > 0) {
                alert("Sorry You Cant Add An Item More Than One Time");
                $(selectObject).val(null).trigger('change');
            }
            else {
                itemUpdatepay(selectObject, dataid, action);
            }
        }
    }
}

function itemUpdatepay(selectObject, dataid, action) {
    var entry = "";
    var url = "";
    var payfrom = $("#ddlpayto").val();
    //if (action == "edit") {
    //    url = '/Receipt/GetReceiptBill';
    //    sentry = getQueryString('');
    //} else {
    url = '/Accounts/SearchAccountsByIdpay';
    entry = selectObject.value;
    //}

    $.ajax({
        url: url,
        dataType: 'json',
        data: { account: payfrom, entry: entry },
        cache: true,
        success: function (data) {

            $("#newrefname_" + dataid).val(data.text);


            $("#invoice_balance_" + dataid).val(data.Balance);
            if (data.Date != null) {
                $("#invoice_date_" + dataid).val(convertToDate(data.Date));
            }
            $("#invoice_balance_" + dataid).val(parseFloat(data.Balance).toFixed(2));

            //var amt = data.Balance < data.Amount ? data.Balance : data.Amount;
            if (action == "edit") {
                $("#invoice_amt_" + dataid).val(parseFloat(data.Amount).toFixed(2));
            } else {
                $("#invoice_amt_" + dataid).val(parseFloat(data.Balance).toFixed(2));
            }

            if (data.Amount > 0) {
                $("#Paying").prop('disabled', true);
            } else {
                $("#Paying").prop('disabled', false);
            }

            //$("#invoice_amt_" + dataid).val(parseFloat(data.Amount).toFixed(2));

            $("#invoice_type_" + dataid).val(data.type);
            // $("#invoice_balanceamt_" + dataid).val(data.BalAmt);

            $(selectObject).closest('tr').attr('class', "invoice_" + data.id);

            if (data.id != null && data.Amount > 0) {
                var count = 0;
                $("#addinvoiceItempay tr").each(function () {
                    var classname = $(this).closest('tr').attr('class');
                    if (classname == 'invoice_') {
                        count++;
                    }
                });
                if (count == 0)
                    addrowspay('addinvoiceItempay', 'new', '', '', '', '0.00', '0.00', '', '');
            }
            RowTotal();
        }
    });
}

function invoice_amt_changepay(arg) {
    var bal = $("#invoice_balance_" + arg).val();
    var amt = $("#invoice_amt_" + arg).val();
    var type = $("#type_name_" + arg).val();

    if (type == "Against Reference") {
        if (parseFloat(bal) < parseFloat(amt)) {
            alert("Amount Should Less than or Equals to Balance Amount..!!");
            $("#invoice_amt_" + arg).val(parseFloat(bal).toFixed(2));
        }
    }

    if (parseFloat(amt) > 0) {
        $("#Paying").prop('disabled', true);
    } else {
        $("#Paying").prop('disabled', false);
    }
    $("#invoice_amt_" + arg).closest('tr').attr('class', "invoice_" + arg);


    //--------check empty rows----------------------------------------
    var count = 0;
    $("#addinvoiceItempay tr").each(function () {
        var classname = $(this).closest('tr').attr('class');
        if (classname == 'invoice_') {
            count++;
        }
    });

    if (count == 0)
        addrows('addinvoiceItempay', 'new', '', '', '', '0.00', '0.00', '', '');
    //-------------------------------------------------------------------

    initialload();

    RowTotal();
    cashBalance();
}

//Delete a row of table
function deleteRowpay(t, item) {
    var classname = $(t).closest('tr').attr('class');
    if (classname == 'invoice_') alert("Sorry you can't delete this row.");
    else {
        var r = confirm("Are you sure you want to delete this..?");
        if (r == true) {
            var e = t.parentNode.parentNode;
            e.parentNode.removeChild(e);
        }
    }
    RowTotal();
    var i = 1;
    $('#addinvoiceItem tr').each(function () {
        $(this).find('td:first').text(i);
        i++;
    });
    chkAmount();
    cashBalance();
}
function grandTotal() {
    var subtotal = parseFloat($('#SubTotal').val());
    var taxamount = parseFloat($('#taxamount').val());
    var discount = parseFloat($('#Discount').val());
    //alert(subtotal);
    subtotal = subtotal || 0;
    taxamount = taxamount || 0;
    discount = discount || 0;

    var grandtotal = subtotal + taxamount + discount;

    $('#GrandTotal').val(grandtotal.toFixed(2));
    $('#Paying').val(grandtotal.toFixed(2));
    $('#Balance').val(0);
    $("#GrandTotal").prop('min', 0.01);
}