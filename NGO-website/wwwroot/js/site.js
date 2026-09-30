/**
 * GIVE-AID Platform Global Client Script
 */
$(document).ready(function () {
    // 1. Cascading Dropdown: NGO -> Causes
    $('#ngoSelect').on('change', function () {
        const ngoId = $(this).val();
        const $causeSelect = $('#causeSelect');
        const $campaignSelect = $('#campaignSelect');

        $causeSelect.empty().append('<option value="">-- Select Welfare Cause --</option>');
        $campaignSelect.empty().append('<option value="">-- None / General Cause Donation --</option>');

        if (ngoId) {
            $.getJSON('/Donate/GetCausesByNgo?ngoId=' + ngoId, function (data) {
                if (data && data.length > 0) {
                    $.each(data, function (i, item) {
                        $causeSelect.append($('<option>', {
                            value: item.causeId,
                            text: item.name + ' (' + item.category + ')'
                        }));
                    });
                }
            });
        }
    });

    // 2. Cascading Dropdown: Cause -> Campaigns
    $('#causeSelect').on('change', function () {
        const causeId = $(this).val();
        const $campaignSelect = $('#campaignSelect');

        $campaignSelect.empty().append('<option value="">-- None / General Cause Donation --</option>');

        if (causeId) {
            $.getJSON('/Donate/GetCampaignsByCause?causeId=' + causeId, function (data) {
                if (data && data.length > 0) {
                    $.each(data, function (i, item) {
                        $campaignSelect.append($('<option>', {
                            value: item.campaignId,
                            text: item.title + ' (Goal: PKR ' + item.goalAmount.toLocaleString() + ')'
                        }));
                    });
                }
            });
        }
    });

    // 3. Donation Amount Preset Pills
    $('.amount-btn-pill').on('click', function () {
        $('.amount-btn-pill').removeClass('active');
        $(this).addClass('active');

        const val = $(this).data('amount');
        $('#donationAmountInput').val(val);
        $('#modalDonationAmountSummary').text('PKR ' + parseFloat(val).toLocaleString());
        $('#modalDonationAmountSummaryJC').text('PKR ' + parseFloat(val).toLocaleString());
    });

    $('#donationAmountInput').on('input', function () {
        const val = $(this).val();
        $('.amount-btn-pill').removeClass('active');
        $('.amount-btn-pill[data-amount="' + val + '"]').addClass('active');

        if (val && !isNaN(val)) {
            $('#modalDonationAmountSummary').text('PKR ' + parseFloat(val).toLocaleString());
            $('#modalDonationAmountSummaryJC').text('PKR ' + parseFloat(val).toLocaleString());
        }
    });

    // 4. Payment Method Switcher
    $('input[name="PaymentMethod"]').on('change', function () {
        const method = $(this).val();
        $('.payment-method-card').removeClass('selected');
        $(this).closest('.payment-method-card').addClass('selected');

        if (method === 'CreditDebitCard' || method === '1') {
            $('#cardPaymentSection').slideDown();
            $('#easyPaisaSection').slideUp();
            $('#jazzCashSection').slideUp();
        } else if (method === 'EasyPaisa' || method === '2') {
            $('#cardPaymentSection').slideUp();
            $('#easyPaisaSection').slideDown();
            $('#jazzCashSection').slideUp();
            $('#easyPaisaModal').modal('show');
        } else if (method === 'JazzCash' || method === '3') {
            $('#cardPaymentSection').slideUp();
            $('#easyPaisaSection').slideUp();
            $('#jazzCashSection').slideDown();
            $('#jazzCashModal').modal('show');
        }
    });

    // 5. EasyPaisa Modal Confirm to main form
    $('#epModalConfirmBtn').on('click', function () {
        const txId = $('#epModalTxId').val().trim();
        const sName = $('#epModalSenderName').val().trim();
        const sMobile = $('#epModalSenderMobile').val().trim();
        const pDate = $('#epModalPaymentDate').val();

        if (!txId) {
            alert('Please enter your EasyPaisa Transaction ID.');
            $('#epModalTxId').focus();
            return;
        }

        if (!sMobile) {
            alert('Please enter the Sender Mobile Number.');
            $('#epModalSenderMobile').focus();
            return;
        }

        $('#hiddenEpTxId').val(txId);
        $('#hiddenEpSenderName').val(sName);
        $('#hiddenEpSenderMobile').val(sMobile);
        if (pDate) $('#hiddenEpPaymentDate').val(pDate);

        // Copy file input reference if present
        const modalFileInput = document.getElementById('epModalReceiptFile');
        const mainFileInput = document.getElementById('mainReceiptFile');
        if (modalFileInput && modalFileInput.files && modalFileInput.files.length > 0 && mainFileInput) {
            mainFileInput.files = modalFileInput.files;
            $('#epReceiptFileNameDisplay').text(modalFileInput.files[0].name);
        }

        $('#epSummaryTxDisplay').text(txId);
        $('#epSummaryMobileDisplay').text(sMobile);
        $('#epConfirmationPreview').removeClass('d-none');

        $('#easyPaisaModal').modal('hide');
    });

    // 6. JazzCash Modal Confirm to main form
    $('#jcModalConfirmBtn').on('click', function () {
        const txId = $('#jcModalTxId').val().trim();
        const sName = $('#jcModalSenderName').val().trim();
        const sMobile = $('#jcModalSenderMobile').val().trim();
        const pDate = $('#jcModalPaymentDate').val();

        if (!txId) {
            alert('Please enter your JazzCash Transaction ID.');
            $('#jcModalTxId').focus();
            return;
        }

        if (!sMobile) {
            alert('Please enter the Sender Mobile Number.');
            $('#jcModalSenderMobile').focus();
            return;
        }

        $('#hiddenJcTxId').val(txId);
        $('#hiddenJcSenderName').val(sName);
        $('#hiddenJcSenderMobile').val(sMobile);
        if (pDate) $('#hiddenJcPaymentDate').val(pDate);

        const modalFileInput = document.getElementById('jcModalReceiptFile');
        const mainFileInput = document.getElementById('mainReceiptFile');
        if (modalFileInput && modalFileInput.files && modalFileInput.files.length > 0 && mainFileInput) {
            mainFileInput.files = modalFileInput.files;
            $('#jcReceiptFileNameDisplay').text(modalFileInput.files[0].name);
        }

        $('#jcSummaryTxDisplay').text(txId);
        $('#jcSummaryMobileDisplay').text(sMobile);
        $('#jcConfirmationPreview').removeClass('d-none');

        $('#jazzCashModal').modal('hide');
    });

    // 7. Prevent Duplicate Form Submissions
    $('form.prevent-duplicate-submit').on('submit', function () {
        const $btn = $(this).find('button[type="submit"]');
        if ($(this).valid && !$(this).valid()) {
            return true;
        }
        $btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span> Processing...');
    });
});
