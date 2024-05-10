var Duracellko;
(function (Duracellko) {

    // Class to handle HTML form to sent email
    var ContactForm = (function () {
        function ContactForm(formId) {
            this._sendEmailUrl = "https://duracellko-functions.azurewebsites.net/api/SendEmail";
            this._formId = formId;
        }

        ContactForm.prototype.init = function () {
            var form = $("#" + this._formId);

            this._contactFormName = form.find("#contactFormName");
            this._contactFormEmail = form.find("#contactFormEmail");
            this._contactFormSubject = form.find("#contactFormSubject");
            this._contactFormMessage = form.find("#contactFormMessage");
            this._sendButton = form.find("button");
            this._alertPanels = new AlertPanelsCollection(form);

            var _this = this;
            form.submit(function (event) {
                return _this.onSubmit(event);
            });

            // Shows the form and hides "Loading..." paragraph
            form.show();
            form.prevAll().hide();

            this._form = form;
        }

        ContactForm.prototype.onSubmit = function (event) {
            event.preventDefault();

            this._alertPanels.hideAll();

            var emailData = this.createEmailData();
            if (this.validate(emailData)) {
                this.sendEmail(emailData);
            }

            return false;
        }

        // Creates EmailData object from form fields. The JSON object will be posted to web service.
        ContactForm.prototype.createEmailData = function () {
            return {
                senderName: this._contactFormName.val(),
                senderEmail: this._contactFormEmail.val(),
                subject: this._contactFormSubject.val(),
                message: this._contactFormMessage.val()
            }
        }

        // Validates that none of the fields are empty.
        ContactForm.prototype.validate = function (emailData) {
            var result = emailData.senderName !== '' &&
                emailData.senderEmail !== '' &&
                emailData.subject !== '' &&
                emailData.message !== '';

            if (!result) {
                this._alertPanels.showValidationError();
            }

            return result;
        }

        // Sends HTTP POST request with EmailData in JSON format.
        ContactForm.prototype.sendEmail = function (emailData) {
            this._sendButton.button('loading');

            var _this = this;
            $.ajax(this._sendEmailUrl, {
                method: "POST",
                contentType: "application/json; charset=UTF-8",
                data: JSON.stringify(emailData)
            })
                .done(function () {
                    _this._alertPanels.showSuccess();
                    _this.clearForm();
                })
                .fail(function (jqXHR) {
                    if (jqXHR.status === 400) {
                        _this._alertPanels.showValidationError();
                    }
                    else {
                        _this._alertPanels.showError();
                    }
                })
                .always(function () {
                    _this._sendButton.button('reset');
                })
        }

        ContactForm.prototype.clearForm = function () {
            this._contactFormName.val('');
            this._contactFormEmail.val('');
            this._contactFormSubject.val('');
            this._contactFormMessage.val('');
        }

        return ContactForm;
    }());
    Duracellko.ContactForm = ContactForm;

    // Class to display and hide alerts and messages.
    var AlertPanelsCollection = (function () {
        function AlertPanelsCollection(form) {
            this.success = form.find("#contactForm-alert-success");
            this.error = form.find("#contactForm-alert-error");
            this.validationError = form.find("#contactForm-alert-validationError");
        }

        AlertPanelsCollection.prototype.hideAll = function () {
            this.success.hide();
            this.error.hide();
            this.validationError.hide();
        }

        AlertPanelsCollection.prototype.showSuccess = function () {
            this.hideAll();
            this.success.slideDown();
        }

        AlertPanelsCollection.prototype.showError = function () {
            this.hideAll();
            this.error.slideDown();
        }

        AlertPanelsCollection.prototype.showValidationError = function () {
            this.hideAll();
            this.validationError.slideDown();
        }

        return AlertPanelsCollection;
    }());
    Duracellko.AlertPanelsCollection = AlertPanelsCollection;

})(Duracellko || (Duracellko = {}));
