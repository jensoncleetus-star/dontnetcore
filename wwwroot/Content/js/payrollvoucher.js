var edcount = 0;

function AddEmployee(EMPId, EMPName) {

    var previouscount = parseFloat(edcount) - 1;
    var previous = $('#item_empl_' + previouscount).val();
    var req = "";
    var Option = "";
    if (edcount == 0 || previous != "") {
        var edumod = $("input[name='employee[" + previouscount + "].Employee']").val();

        if (EMPId != null) {
            Option = "<option value='" + EMPId + "'>" + EMPName + "</option>";
        }

        if (EMPId != null) {
            row = "<div class='empSet emps_" + EMPId + "' id='empset_" + count + "'>";
        } else {
            row = "<div class='empSet emps_' id='empset_" + count + "'>";
        }

        //if (edcount == 0 || edumod != "") {
           // var html = '<div class="empSet">' +
            var html = row + '<div class="col-sm-6 form-group"><div class="input-group input-group-sm">' +
            '<label class="control-label">Employees </label>' +
            '<select data-name="EMPName" class="form-control item_empl" placeholder="Employee Name" id="item_empl_' + edcount + '" data-msg-required="The Employee Name field is required" onchange="GetEmpDetail(this,' + edcount + ')">' + Option + '</select>'+
            '<span class="input-group-btn lblpadding"><button class="btn btn-danger" type="button" value="Delete" onclick="deleteRow(this,' + edcount + ')"><i class="fa fa-trash fa-1x"></i></button></span>' +
            '<input type="hidden" class="hidemp" value=' + EMPId + ' name="employee[].Employee" id="empmain_' + edcount + '" />' +
            '</div>' +
            '</div>' 
            $(html).appendTo($("#payemptable"));
            edcount++;
            resetBtn();
            searchItem();
        //}
    }
}
function GetEmpDetail(selectObject, dataid) {
    if (selectObject.value) {
        var EmpId = selectObject.value;
        var chkval = false;
        if (EmpId != null) {
            if ($(".emps_" + EmpId).length > 0) {
                alert("Sorry You Cant Add An Employee More Than One Time..!!");
                $(selectObject).val(null).trigger('change');
            }
            else {
                $(selectObject).closest('div').parent().parent().attr('class', "empSet emps_" + EmpId);
                $(selectObject).prop('disabled', true);
                $("#empmain_" + dataid).val(EmpId);
                itemUpdate(selectObject, dataid);
                AddEmployee();
            }
        }
    } else {
       // $(selectObject).closest('tr').attr('class', "empl_");
    }
  
   // removeVacant();
}


function itemUpdate(selectObject, dataid) {
    var EmpID = selectObject.value;
    var FDate = $("#FromDate").val();
    var TDate = $("#ToDate").val();
    if (EmpID > 0 && FDate != "" && TDate != "") {
        $.ajax({
            url: '/Hr/SalaryStructure/GetSalaryStrDetails',
            dataType: 'json',
            type: "POST",
            data: { EmpID: EmpID, FDate: FDate, TDate: TDate },
            cache: true,
            async: false,
            success: function (data) {

                //var price = 0;
                //var seldate = $("#PRDate").val().split('-');
                //var dateYear = parseInt(seldate[2]);
                //var dateMonth = parseInt(seldate[1]);
                //var dateDay = parseInt(seldate[0]);
                //var monthdays = daysInMonth(dateMonth, dateYear);
                addemphead(EmpID);
                var totrate = 0;
                $.each(data.sal, function (i, item) {
                    //var prate = 0;
                    //var rateprice = 0;
                    //var type = "";
                    //if (item.HeadType == "EarningsforEmployees") {
                    //    type = "Dr";
                    //    if (item.CalType == "OnAttendance") {
                    //        if (item.CalculationPeriod == "Months") {
                    //            //
                    //            if (item.Basis == "AsperCalenderPeriod") {
                    //                if (item.AtType == 1 && item.Leave == "Absent") {
                    //                    if (item.AtUnit == "Days") {
                    //                        var monthsal = parseFloat(item.Rate / monthdays);
                    //                        var minval = monthsal * item.AtValue;
                    //                        rateprice = parseFloat(item.Rate - minval);
                    //                    }
                    //                }
                    //                if (item.AtType == 4 && item.Leave == "PRESENT") {
                    //                    if (item.AtUnit == "Days") {
                    //                        var monthsal = parseFloat(item.Rate / monthdays);
                    //                        var minval = monthsal * item.AtValue;
                    //                        rateprice = parseFloat(minval);
                    //                    }
                    //                }
                    //            }
                    //            //
                    //            if (item.Basis == "UserDefined") {
                    //                if (item.AtType == 1 && item.Leave == "Absent") {
                    //                    if (item.AtUnit == "Days") {
                    //                        var monthsal = parseFloat(item.Rate / item.days);
                    //                        var minval = monthsal * item.AtValue;
                    //                        rateprice = parseFloat(item.Rate - minval);
                    //                    }
                    //                }
                    //                if (item.AtType == 4 && item.Leave == "PRESENT") {
                    //                    if (item.AtUnit == "Days") {
                    //                        var monthsal = parseFloat(item.Rate / item.days);
                    //                        var minval = monthsal * item.AtValue;
                    //                        rateprice = parseFloat(minval);
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //    if (item.CalType == "FlatRate") {
                    //        rateprice = item.Rate;
                    //    }
                    //    if (item.CalType == "DefinedValue") {
                    //        rateprice = 0.00;
                    //    }
                    //    if (item.CalType == "OnProduction") {
                    //        if (item.AtType == 3)//overtime hrs
                    //        {
                    //            var minval = item.Rate * item.AtValue;
                    //            rateprice = parseFloat(minval);
                    //        }
                    //    }

                    //}
                    //if (item.HeadType == "DeductionsfromEmployees") {
                    //    type = "Cr";
                    //    if (item.CalType == "FlatRate") {
                    //        rateprice = item.Rate;
                    //    }
                    //    if (item.CalType == "DefinedValue") {
                    //        rateprice = 0.00;
                    //    }
                    //}
                    //if (item.HeadType == "LoansandAdvances") {
                    //    type = "Cr";
                    //    if (item.CalType == "DefinedValue") {
                    //        rateprice = 0.00;
                    //    }
                    //}
                    //if (item.HeadType == "EmployeesStatutoryContributions") {

                    //    if (item.CalType == "AsComputedValue") {
                    //        $.each(item.compute, function (i, itemz) {
                    //            if (itemz.Amountgreatethan < item.Rate < itemz.Amountupto) {
                    //                if (item.Computed == "DeductionsTotal") {
                    //                    type = "Cr";
                    //                }
                    //                if (item.Computed == "EarningsTotal") {
                    //                    type = "Dr";
                    //                }
                    //                //if (item.Computed == "CurrentSubtotal") {

                    //                //}
                    //                //if (item.Computed == "SpecifiedFormula") {

                    //                //}
                    //                if (itemz.Slabtype == 1) {//percentage
                    //                    rateprice = (item.Rate) * (itemz.value / 100);
                    //                } else {//value
                    //                    rateprice = itemz.value;
                    //                }
                    //            }
                    //        });
                    //    }
                    //}

                    $('#addempitem').html('');
                    var rateprice = parseInt(item.rateprice);
                    //addempitem('addempitem', '', item.PayHead, item.PayHeadId, rateprice, item.EmpId, item.EmpName, type, item);
                    addempitem('addempitem', '', item.PayHead, item.PayHeadId, rateprice, item.EmpId, item.EmpName, item.type,item.CalType, item);
                    totrate += rateprice;
                });


                $("#addempitemfoot_" + EmpID).val(totrate.toFixed(2));
                CalculateTotal();
                //resetBtn();
                //$("#pheaddiv").show();
                $(".grtoal").show();
                //if ($(".empl_").length == 0) {
                //    //funemployee('emplbody', '', '0');
                //}
            }
        });
    } else {
        alert("Please select Employee, From , To dates ..!!");
        location.reload();
        //$("#addempitemh_" + EmpID).remove();
    }
}
function daysInMonth(month, year) {
    return new Date(year, month, 0).getDate();
}

function addempitem(t, action, PayHead, PayHeadId, Rate, EmpId, EmpName, CrDr,CalType, Item) {
    if (count == limits) alert("You have reached the limit of adding " + count + " inputs");
    else {
        var data = "";
        var Type = "";
        var readonly = "";
        var row = "";
        var drcr = "";
        var Option = "";
        var readon = "";
        if (PayHeadId != null) {
            row = "<tr class='salstr emp_" + PayHeadId + "' id='emp_" + count + "'>";
        } else {
            row = "<tr class='salstr emp_' id='emp_" + count + "'>";
        }

        var slno = $('#addempitem_' + EmpId + ' tr').length + 1;

        tabindex = count * 4;
        tab1 = tabindex + 1;
        tab2 = tabindex + 2;
        tab3 = tabindex + 3;
        tab4 = tabindex + 4;
        tab5 = tabindex + 5;
        tab6 = tabindex + 6;

        var required = "";
        if (count == 1) {
            required = 'required="required"';
        }
        if (action != '') {
            type = action;
        }

        if (Item == null) {
            var Option = "<option value='Dr'>Dr</option><option value='Cr'>Cr</option>";
        } else {
            if (CrDr == "Cr") {
                drcr = "Cr";
                Option = "<option value='Dr'>Dr</option><option selected='selected' value='Cr'>Cr</option>";
            } else {
                drcr = "Dr";
                Option += "<option selected='selected' value='Dr'>Dr</option><option value='Cr'>Cr</option>";
            }

            if (CalType == "FlatRate" || CalType == "DefinedValue" || CalType == 0 || CalType == 2) {
                readon = "";
            } else {
                readon = "readonly ='readonly'";
            }

        }

        Rate = Rate != null ? Rate : 0;

        data = "<td class='text-center'> " + slno + " </td>" +
               //"<td><input type='text' id='empname_" + count + "' value='" + EmpName + "'  class='empname_" + count + " form-control text-center'  tabindex='" + tab1 + "' readonly='readonly'/></td>" +
               "<td><input type='text' id='payhead_" + count + "' value='" + PayHead + "'  class='payhead_" + count + " form-control text-center'  tabindex='" + tab2 + "' readonly='readonly'/></td>" +
               "<td><input type='number' name='salarystr[].Rate' id='value_" + count + "' " + readon + " value='" + Rate.toFixed(2) + "'  class='totrate value_" + count + " form-control text-right Rate PRate_" + EmpId + "' placeholder='0' value='0' onchange='itemrate_change(" + count + "," + EmpId + ");' tabindex='" + tab3 + "'/></td>" ;
        //if (readon != "")
        //{
        data += "<td><input type='text' name='salarystr[].CrDr' id='crdr_" + count + "' data-id='" + count + "' value=" + drcr + "  class='form-control CrDr crdr_" + count + "' text-center'  tabindex='" + tab4 + "' readonly ='readonly' /></td>";
        //}else{
        //    data += "<td><select name='salarystr[].CrDr' class='form-control CrDr crdr_" + count + "' data-id='" + count + "' id='crdr_" + count + "' tabindex='" + tab4 + "' onchange='itemcrdr_change(" + count + "," + EmpId + ");'>" + Option + "</select>";
        //}
        data += "<td><input type='text' id='cbalance_" + count + "' value=''  class='cbalance_" + count + " form-control text-center'  tabindex='" + tab5 + "' readonly='readonly'/>" +
               "<input type='hidden' name='salarystr[].EmpId' class='empid empid_" + count + "' value='" + EmpId + "'  id='empid_" + count + "'/>" +
               "<input type='hidden' name='salarystr[].PayHeadId' class='payheadid payhead_" + count + "' value='" + PayHeadId + "'  id='payhead_" + count + "'/></td>";
        row += data + "</tr>";

        $('#addempitem_' + EmpId).append(row);

        count++;
        //setTabIndex();
        CalculateTotal();
        resetBtn();

    }
}
function resetBtn() {
    var j = 0;
    $('.empSet').each(function (index, element) {
        var i = 0;
        var input0 = $(this).find('.hidemp')
        input0.attr('name', 'employee[' + j + '].Employee');

        $('.salstr').each(function (index, element) {

            var input1 = $(this).find('.totrate')
            input1.attr('name', 'salarystr[' + i + '].Rate');

            var input2 = $(this).find('.empid');
            input2.attr('name', 'salarystr[' + i + '].EmpId');

            var input3 = $(this).find('.payheadid');
            input3.attr('name', 'salarystr[' + i + '].PayHeadId');

            var input4 = $(this).find('.CrDr');
            input4.attr('name', 'salarystr[' + i + '].CrDr');

            i++;
        });
        j++;
    });

}
//delete function
function deleteRow(t, dataid) {
    var bm = $("#item_empl_" + dataid).val(); //$(t).closest('tr').attr('class');//
    var classname = $(t).closest('div').parent().parent().attr('class');
    if (classname == 'empSet emps_') alert("Sorry you can't delete this row.");
    else {
        var r = confirm("Are you sure you want to delete this..?");
        if (r == true) {
            
            //delete item
            var tbodyitem = $("#normalinvoice tbody");
            if (tbodyitem.children().length > 0) {
                tbodyitem.children("tr").each(function () {
                    var rowid = $(this).attr("id");
                    var empid = $("#" + rowid + " .empid").val();
                    if (bm == empid) {
                        $(this).closest("tr").remove();
                    }
                });
            }
            $("#addempitemh_" + bm).remove();
            $("#addempitem_" + bm).remove();
            $("#footaddempitem_" + bm).remove();

            //delete emp
            var e = t.parentNode.parentNode.parentNode.parentNode;
            e.parentNode.removeChild(e);

            if (tbodyitem.children().length == 0) {
                $(".grtoal").hide();
            }


            var j = 1;
            $('#addempitem tr').each(function () {
                $(this).find('td:first').text(j);
                j++;
            });
        }
    }
    CalculateTotal();
    resetBtn();

}
function addemphead(id) {
    var head = "<table class='table table-bordered table-hover' id='normalinvoice'><thead id='addempitemh_" + id + "'><tr>" +
               "<th class='text-center'>S/N</th><th class='text-center'>Pay Head</th><th class='text-center'>Amount</th><th class='text-center'>Dr/Cr</th><th class='text-center'>Current Balance</th>" +
               "</tr></thead><tbody id='addempitem_" + id + "'></tbody><tfoot id='footaddempitem_" + id + "'><td></td><td></td><td></td><td class='text-right'><b>Total :</b></td><td><input id='addempitemfoot_" + id + "' type='number' class='form-control tfootsub text-right' readonly='readonly' /></td></tfoot></table>";

    $('#payemptable').append(head);
}

function searchItem() {
    var selecteditem = new Array();
    $(".item_empl").each(function () {
        selecteditem.push($(this).val());
    });

    $(".item_empl").select2({
        placeholder: 'Search by Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/SalaryStructure/SearchParentGrade",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0,
                    //x: "empty"
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
function CalculateTotal() {
    var tbody = $("#normalinvoice tbody");
    if (tbody.children().length > 0) {
        var gtSubTotal = 0;

        $(".tfootsub").each(function () {
            var subTot = this.value;
            gtSubTotal = parseFloat(gtSubTotal) + parseFloat(subTot);
        });
        gtSubTotal = gtSubTotal || 0.00;
        $("#GrandTotal").val(gtSubTotal.toFixed(2));
    }
}
function itemrate_change(arg, empid) {
    calsubtotal(arg, empid);
    CalculateTotal();
}
function itemcrdr_change(arg, empid)
{
    calsubtotal(arg, empid);
    CalculateTotal();
}

function calsubtotal(arg, empid) {
    var TotVal = 0;
    var tbody = $("#normalinvoice tbody");
    if (tbody.children().length > 0) {
        tbody.children("tr").each(function () {
            var rowid = $(this).attr("id");
            var rate = $("#" + rowid + " .Rate").val();
            var emp = $("#" + rowid + " .empid").val();
            var crdr = $("#" + rowid + " .CrDr ").val();
            if (emp == empid) {
                if (crdr == "Dr") {
                    TotVal = parseFloat(TotVal) + parseFloat(rate);
                }
                if (crdr == "Cr") {
                    TotVal = parseFloat(TotVal) - parseFloat(rate);
                }
            }
        });
        $("#addempitemfoot_" + empid).val(TotVal.toFixed(2));
    }
}