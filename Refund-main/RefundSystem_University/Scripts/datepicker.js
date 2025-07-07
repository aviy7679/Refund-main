jQuery(function ($) {
    $.validator.addMethod('date',
        function (value, element) {
            $.culture = Globalize.culture("he-IL");
            var date = Globalize.parseDate(value, "dd/MM/yyyy", "he-IL");
            return this.optional(element) ||
                !/Invalid|NaN/.test(new Date(date).toString());
        });

    $(".datepicker").datepicker({
        changeYear: true,
        //yearRange: "1930:2030", //The default is a range of 10 years from the current year
        changeMonth: true,
        dateFormat: 'dd/mm/yy',
    });
});