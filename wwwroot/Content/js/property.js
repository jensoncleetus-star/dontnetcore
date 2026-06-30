
function GetDeveloper(val) {

    var type =( val == 0) ? "empty" : "all";
    $("#ddlDeveloper").select2({
        placeholder: 'Search Developer Name or Code',
        minimumInputLength: 0,
        ajax: {
            url: "/Property/Developer/SearchDeveloper",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: type,
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

function GetOwner(val) {
    var type =( val == 0) ? "empty" : "all";
    $("#ddlOwner").select2({
        placeholder: 'Search Owner By Code or name',
            minimumInputLength: 0,
            ajax: {
                //url: "/Accounts/SearchAccounts",
                url: "/Accounts/AllAccountsLand",
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

function GetProperty(val) {
    var type = (val == 0) ? "empty" : val;
    $("#ddlProperty").select2({
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
                    x: type,
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
function GetBroker(val) {
    var type =( val == 0) ? "empty" : "all";
    $("#ddlBroker").select2({
        placeholder: 'Search Broker Name or Code',
        minimumInputLength: 0,
        ajax: {
            url: "/Property/Broker/SearchBroker",
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

function PropertyPopup()
{
    $('#modal-property').on('shown.bs.modal', function (e) {
       

        $.fn.modal.Constructor.prototype.enforceFocus = function () { };
        $('#modal-property').on('shown.bs.modal', function (e) {

        });

        AddMobile();
    });
}

function GetTenant(val) {

    var type =( val == 0) ? "empty" : "all";
    $("#ddlTenant").select2({
        placeholder: 'Search Customer by Name or Code',
        minimumInputLength: 0,
        ajax: {
            url: "/Customer/SearchCustomer",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: type,
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
    $("#ddlUnit").select2({
        placeholder: 'Search Unit by Name',
        minimumInputLength: 0,
        ajax: {
            url: "/Property/Unit/SearchUnit",
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

function GetPropertyforPayment(val) {
    var type =( val == 0) ? "empty" : "all";
    $("#ddlProject").select2({
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
function GetUnitforPayment(val) {
    $("#ddlProTask").select2({
        placeholder: 'Search Unit by Name',
        minimumInputLength: 0,
        ajax: {
            url: "/Property/Unit/SearchUnit",
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

function GetFeature(val) {
    $("#ddlFeature").select2({
        placeholder: 'Search Feature by Name',
        minimumInputLength: 0,
        ajax: {
            url: "/Property/PropertyFeature/SearchFeature",
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
function GetDocumentType(val) {
    $("#ddlDocumentType").select2({
        placeholder: 'Search Document Type by Name',
        minimumInputLength: 0,
        ajax: {
            url: "/Property/DocumentType/SearchDocumentType",
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

function GetPropertywithOwner(val) {
    var type =( val == 0) ? "empty" : "all";
    $("#ddlProperty").select2({
        placeholder: 'Search Property Name or Code',
        minimumInputLength: 0,
        ajax: {
            url: "/Property/PropertyMain/SearchPropertyWithOwner",
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
       templateResult: SelectToGroup,
    templateSelection: ToSetFormatSelection,
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

function GetContractor(val) {

    var type =( val == 0) ? "empty" : "all";
    $("#ddlContractor").select2({
        placeholder: 'Search Contractor Name or Code',
        minimumInputLength: 0,
        ajax: {
            url: "/Property/Contractor/SearchContractor",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: type,
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

function GetLandlord(val) {

    var type =( val == 0) ? "empty" : "all";
    $("#ddlLandlord").select2({
        placeholder: 'Search Landlord Name or Code',
        minimumInputLength: 0,
        ajax: {
            url: "/Property/Landlords/SearchLandlord",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: type,
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

function GetUnitType(val) {

    var type =( val == 0) ? "empty" : "all";
    $("#ddlUnitType").select2({
        placeholder: 'Search Unit Type',
        minimumInputLength: 0,
        ajax: {
            url: "/Property/UnitType/SearchUnitType",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: type,
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

function GetPropertyType(val) {

    var type =( val == 0) ? "empty" : "all";
    $("#ddlPropertyType").select2({
        placeholder: 'Search Property Name or Code',
        minimumInputLength: 0,
        ajax: {
            url: "/Property/PropertyType/SearchPropertyType",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: type,
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

function GetLandlordAll(val) {

    var type = (val == 0) ? "empty" : "all";
    $("#ddlLandlord").select2({
        placeholder: 'Search Landlord Name or Code',
        minimumInputLength: 0,
        ajax: {
            url: "/Property/Landlords/SearchLandlordAll",
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

function GetDuration(val) {
    $("#ddlDuration").select2({
        placeholder: 'Search Duration by Name',
        minimumInputLength: 0,
        ajax: {
            url: "/Property/Duration/SearchDuration",
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

function GetPropertyNotRegistered(val) {
    var type = (val == 0) ? "empty" : val;
    $("#ddlProperty").select2({
        placeholder: 'Search Property Name or Code',
        minimumInputLength: 0,
        ajax: {
            url: "/Property/PropertyMain/SearchPropertyNotReg",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: type,
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
