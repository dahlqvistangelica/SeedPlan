using FluentResults;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SeedPlan.Client.Services;
using System;
using System.Threading.Tasks;

namespace SeedPlan.Client.Services.UnitTests
{
    [TestClass]
    public class AuthServiceTests
    {
        [TestMethod]
        public async Task LoginAsync_ValidSessionAndRememberMe_StoresSessionInLocalStorageAndReturnsOk()
        {
            var mockAuth = new Mock<IAuthClient>();
            var mockNotifier = new Mock<IAuthStateNotifier>();
            var mockJs = new Mock<IJSRuntime>();
            var nav = new TestNavigationManager();

            mockAuth.Setup(a => a.SignIn("test@example.com", "password123"))
                .ReturnsAsync(new AuthSignInResult { HasUser = true, AccessToken = "token", SessionJson = "{}" });
            mockJs.Setup(js => js.InvokeAsync<IJSVoidResult>(It.IsAny<string>(), It.IsAny<object?[]?>()))
                .ReturnsAsync(Mock.Of<IJSVoidResult>());
            mockJs.Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object?[]?>()))
                .ReturnsAsync("true");

            var service = new AuthService(mockAuth.Object, mockNotifier.Object, nav, mockJs.Object);

            var result = await service.LoginAsync("test@example.com", "password123", true);

            Assert.IsTrue(result.IsSuccess);
            mockAuth.Verify(a => a.SignIn("test@example.com", "password123"), Times.Once);
            mockNotifier.Verify(n => n.NotifyUserChanged(), Times.Once);
            mockJs.Verify(js => js.InvokeAsync<IJSVoidResult>("localStorage.setItem",
                It.Is<object?[]?>(args => args != null && args.Length == 2 && (string)args[0]! == "sb_session")), Times.Once);
            mockJs.Verify(js => js.InvokeAsync<IJSVoidResult>("localStorage.setItem",
                It.Is<object?[]?>(args => args != null && args.Length == 2 && (string)args[0]! == "sb_remember_me")), Times.Once);
        }

        [TestMethod]
        public async Task LoginAsync_NullSessionOrToken_ReturnsFailure()
        {
            var mockAuth = new Mock<IAuthClient>();
            var mockNotifier = new Mock<IAuthStateNotifier>();
            var mockJs = new Mock<IJSRuntime>();
            var nav = new TestNavigationManager();

            mockAuth.Setup(a => a.SignIn(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new AuthSignInResult { HasUser = false, AccessToken = null });

            var service = new AuthService(mockAuth.Object, mockNotifier.Object, nav, mockJs.Object);

            var result = await service.LoginAsync("test@example.com", "password123", false);

            Assert.IsTrue(result.IsFailed);
            Assert.AreEqual("Inloggning misslyckades.", result.Errors[0].Message);
            mockNotifier.Verify(n => n.NotifyUserChanged(), Times.Never);
        }

        [TestMethod]
        public async Task LoginAsync_MissingSessionJson_ReturnsFailure()
        {
            var mockAuth = new Mock<IAuthClient>();
            var mockNotifier = new Mock<IAuthStateNotifier>();
            var mockJs = new Mock<IJSRuntime>();
            var nav = new TestNavigationManager();

            mockAuth.Setup(a => a.SignIn(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new AuthSignInResult { HasUser = true, AccessToken = "token", SessionJson = null });

            var service = new AuthService(mockAuth.Object, mockNotifier.Object, nav, mockJs.Object);

            var result = await service.LoginAsync("test@example.com", "password123", true);

            Assert.IsTrue(result.IsFailed);
            Assert.AreEqual("Inloggning misslyckades.", result.Errors[0].Message);
            mockNotifier.Verify(n => n.NotifyUserChanged(), Times.Never);
        }

        [TestMethod]
        public async Task LoginAsync_WhenRememberMeFalse_StoresSessionInSessionStorage()
        {
            var mockAuth = new Mock<IAuthClient>();
            var mockNotifier = new Mock<IAuthStateNotifier>();
            var mockJs = new Mock<IJSRuntime>();
            var nav = new TestNavigationManager();

            mockAuth.Setup(a => a.SignIn("test@example.com", "password123"))
                .ReturnsAsync(new AuthSignInResult { HasUser = true, AccessToken = "token", SessionJson = "{}" });
            mockJs.Setup(js => js.InvokeAsync<IJSVoidResult>(It.IsAny<string>(), It.IsAny<object?[]?>()))
                .ReturnsAsync(Mock.Of<IJSVoidResult>());

            var service = new AuthService(mockAuth.Object, mockNotifier.Object, nav, mockJs.Object);

            var result = await service.LoginAsync("test@example.com", "password123", false);

            Assert.IsTrue(result.IsSuccess);
            mockJs.Verify(js => js.InvokeAsync<IJSVoidResult>("sessionStorage.setItem",
                It.Is<object?[]?>(args => args != null && args.Length == 2 && (string)args[0]! == "sb_session")), Times.Once);
            mockJs.Verify(js => js.InvokeAsync<IJSVoidResult>("localStorage.removeItem",
                It.Is<object?[]?>(args => args != null && args.Length == 1 && (string)args[0]! == "sb_session")), Times.Once);
            mockJs.Verify(js => js.InvokeAsync<IJSVoidResult>("localStorage.removeItem",
                It.Is<object?[]?>(args => args != null && args.Length == 1 && (string)args[0]! == "sb_remember_me")), Times.Once);
        }

        [TestMethod]
        public async Task LogoutAsync_WhenCalled_PerformsAllOperations()
        {
            var mockAuth = new Mock<IAuthClient>();
            var mockNotifier = new Mock<IAuthStateNotifier>();
            var mockJs = new Mock<IJSRuntime>();
            var nav = new TestNavigationManager();

            mockAuth.Setup(a => a.SignOut()).Returns(Task.CompletedTask);
            mockJs.Setup(js => js.InvokeAsync<IJSVoidResult>(It.IsAny<string>(), It.IsAny<object?[]?>()))
                .ReturnsAsync(Mock.Of<IJSVoidResult>());

            var service = new AuthService(mockAuth.Object, mockNotifier.Object, nav, mockJs.Object);

            await service.LogoutAsync();

            mockAuth.Verify(a => a.SignOut(), Times.Once);
            mockNotifier.Verify(n => n.NotifyUserChanged(), Times.Once);
            Assert.AreEqual("http://localhost/", nav.LastNavigatedUri);
        }

        [TestMethod]
        public async Task RegisterAsync_ValidInputs_ReturnsOkResult()
        {
            var mockAuth = new Mock<IAuthClient>();
            var mockNotifier = new Mock<IAuthStateNotifier>();
            var mockJs = new Mock<IJSRuntime>();
            var nav = new TestNavigationManager();

            mockAuth.Setup(a => a.SignUp("test@example.com", "password123", It.IsAny<System.Collections.Generic.Dictionary<string, object>>()))
                .ReturnsAsync(true);

            var service = new AuthService(mockAuth.Object, mockNotifier.Object, nav, mockJs.Object);

            var result = await service.RegisterAsync("test@example.com", "password123", "John", "Doe");

            Assert.IsTrue(result.IsSuccess);
            mockAuth.Verify(a => a.SignUp("test@example.com", "password123",
                It.Is<System.Collections.Generic.Dictionary<string, object>>(o => o != null
                    && o.ContainsKey("first_name")
                    && o.ContainsKey("last_name")
                    && o.ContainsKey("full_name")
                    && o["first_name"].ToString() == "John"
                    && o["last_name"].ToString() == "Doe"
                    && o["full_name"].ToString() == "John Doe")), Times.Once);
        }

        [TestMethod]
        [DataRow("  John  ", "  Doe  ", "John     Doe")]
        [DataRow("John", "  ", "John")]
        [DataRow("  ", "Doe", "Doe")]
        [DataRow("  ", "  ", "")]
        public async Task RegisterAsync_WhitespaceInNames_TrimsFullName(string firstName, string lastName, string expectedFullName)
        {
            var mockAuth = new Mock<IAuthClient>();
            var mockNotifier = new Mock<IAuthStateNotifier>();
            var mockJs = new Mock<IJSRuntime>();
            var nav = new TestNavigationManager();

            mockAuth.Setup(a => a.SignUp(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.Collections.Generic.Dictionary<string, object>>()))
                .ReturnsAsync(true);

            var service = new AuthService(mockAuth.Object, mockNotifier.Object, nav, mockJs.Object);

            var result = await service.RegisterAsync("test@example.com", "password123", firstName, lastName);

            Assert.IsTrue(result.IsSuccess);
            mockAuth.Verify(a => a.SignUp(It.IsAny<string>(), It.IsAny<string>(),
                It.Is<System.Collections.Generic.Dictionary<string, object>>(o => o != null
                    && o.ContainsKey("full_name")
                    && o["full_name"].ToString() == expectedFullName)), Times.Once);
        }

        [TestMethod]
        public async Task RegisterAsync_NullResponse_ReturnsFailResult()
        {
            var mockAuth = new Mock<IAuthClient>();
            var mockNotifier = new Mock<IAuthStateNotifier>();
            var mockJs = new Mock<IJSRuntime>();
            var nav = new TestNavigationManager();

            mockAuth.Setup(a => a.SignUp(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.Collections.Generic.Dictionary<string, object>>()))
                .ReturnsAsync(false);

            var service = new AuthService(mockAuth.Object, mockNotifier.Object, nav, mockJs.Object);

            var result = await service.RegisterAsync("test@example.com", "password123", "John", "Doe");

            Assert.IsTrue(result.IsFailed);
            Assert.AreEqual("Registrering misslyckades.", result.Errors[0].Message);
        }

        [TestMethod]
        public async Task RegisterAsync_ResponseUserIsNull_ReturnsFailResult()
        {
            var mockAuth = new Mock<IAuthClient>();
            var mockNotifier = new Mock<IAuthStateNotifier>();
            var mockJs = new Mock<IJSRuntime>();
            var nav = new TestNavigationManager();

            mockAuth.Setup(a => a.SignUp(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.Collections.Generic.Dictionary<string, object>>()))
                .ReturnsAsync(false);

            var service = new AuthService(mockAuth.Object, mockNotifier.Object, nav, mockJs.Object);

            var result = await service.RegisterAsync("test@example.com", "password123", "John", "Doe");

            Assert.IsTrue(result.IsFailed);
            Assert.AreEqual("Registrering misslyckades.", result.Errors[0].Message);
        }

        [TestMethod]
        [DataRow("invalid_credentials", "Fel e-postadress eller lösenord.")]
        [DataRow("email_not_confirmed", "Du måste bekräfta din e-post innan du kan logga in.")]
        [DataRow("user_already_exists", "Det finns redan ett konto med denna e-postadress.")]
        [DataRow("weak_password", "Lösenordet är för svagt. Använd minst 6 tecken.")]
        [DataRow("unknown_error", "Ett oväntat fel uppstod vid inloggningen.")]
        public async Task RegisterAsync_ExceptionThrown_ReturnsFailWithTranslatedError(string errorCode, string expectedMessage)
        {
            var mockAuth = new Mock<IAuthClient>();
            var mockNotifier = new Mock<IAuthStateNotifier>();
            var mockJs = new Mock<IJSRuntime>();
            var nav = new TestNavigationManager();

            mockAuth.Setup(a => a.SignUp(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.Collections.Generic.Dictionary<string, object>>()))
                .ThrowsAsync(new Exception(errorCode));

            var service = new AuthService(mockAuth.Object, mockNotifier.Object, nav, mockJs.Object);

            var result = await service.RegisterAsync("test@example.com", "password123", "John", "Doe");

            Assert.IsTrue(result.IsFailed);
            Assert.AreEqual(expectedMessage, result.Errors[0].Message);
        }

        [TestMethod]
        public async Task UpdateEmailAsync_CurrentUserIsNull_ReturnsFailure()
        {
            var mockAuth = new Mock<IAuthClient>();
            var mockNotifier = new Mock<IAuthStateNotifier>();
            var mockJs = new Mock<IJSRuntime>();
            var nav = new TestNavigationManager();

            mockAuth.SetupGet(a => a.CurrentUserEmail).Returns((string?)null);
            var service = new AuthService(mockAuth.Object, mockNotifier.Object, nav, mockJs.Object);

            var result = await service.UpdateEmailAsync("newemail@example.com", "currentPassword");

            Assert.IsTrue(result.IsFailed);
            Assert.AreEqual("Kunde inte hämta användaruppgifter.", result.Errors[0].Message);
        }

        [TestMethod]
        public async Task UpdateEmailAsync_CurrentUserEmailIsNull_ReturnsFailure()
        {
            var mockAuth = new Mock<IAuthClient>();
            var mockNotifier = new Mock<IAuthStateNotifier>();
            var mockJs = new Mock<IJSRuntime>();
            var nav = new TestNavigationManager();

            mockAuth.SetupGet(a => a.CurrentUserEmail).Returns((string?)null);

            var service = new AuthService(mockAuth.Object, mockNotifier.Object, nav, mockJs.Object);

            var result = await service.UpdateEmailAsync("newemail@example.com", "currentPassword");

            Assert.IsTrue(result.IsFailed);
            Assert.AreEqual("Kunde inte hämta användaruppgifter.", result.Errors[0].Message);
        }

        [TestMethod]
        public async Task UpdateEmailAsync_ValidPasswordAndUpdate_Succeeds()
        {
            var mockAuth = new Mock<IAuthClient>();
            var mockNotifier = new Mock<IAuthStateNotifier>();
            var mockJs = new Mock<IJSRuntime>();
            var nav = new TestNavigationManager();

            mockAuth.SetupGet(a => a.CurrentUserEmail).Returns("old@example.com");
            mockAuth.Setup(a => a.SignIn("old@example.com", "currentPassword"))
                .ReturnsAsync(new AuthSignInResult { HasUser = true, AccessToken = "token", SessionJson = "{}" });
            mockAuth.Setup(a => a.UpdateEmail("new@example.com")).ReturnsAsync(true);

            var service = new AuthService(mockAuth.Object, mockNotifier.Object, nav, mockJs.Object);

            var result = await service.UpdateEmailAsync("new@example.com", "currentPassword");

            Assert.IsTrue(result.IsSuccess);
            mockAuth.Verify(a => a.UpdateEmail("new@example.com"), Times.Once);
        }

        [TestMethod]
        public async Task UpdateEmailAsync_WrongCurrentPassword_ReturnsFailure()
        {
            var mockAuth = new Mock<IAuthClient>();
            var mockNotifier = new Mock<IAuthStateNotifier>();
            var mockJs = new Mock<IJSRuntime>();
            var nav = new TestNavigationManager();

            mockAuth.SetupGet(a => a.CurrentUserEmail).Returns("old@example.com");
            mockAuth.Setup(a => a.SignIn("old@example.com", "wrongPassword"))
                .ThrowsAsync(new Exception("invalid_credentials"));

            var service = new AuthService(mockAuth.Object, mockNotifier.Object, nav, mockJs.Object);

            var result = await service.UpdateEmailAsync("new@example.com", "wrongPassword");

            Assert.IsTrue(result.IsFailed);
            Assert.AreEqual("Nuvarande lösenord är felaktigt.", result.Errors[0].Message);
            mockAuth.Verify(a => a.UpdateEmail(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdateEmailAsync_UpdateEmailReturnsFalse_ReturnsFailure()
        {
            var mockAuth = new Mock<IAuthClient>();
            var mockNotifier = new Mock<IAuthStateNotifier>();
            var mockJs = new Mock<IJSRuntime>();
            var nav = new TestNavigationManager();

            mockAuth.SetupGet(a => a.CurrentUserEmail).Returns("old@example.com");
            mockAuth.Setup(a => a.SignIn("old@example.com", "currentPassword"))
                .ReturnsAsync(new AuthSignInResult { HasUser = true, AccessToken = "token", SessionJson = "{}" });
            mockAuth.Setup(a => a.UpdateEmail("new@example.com")).ReturnsAsync(false);

            var service = new AuthService(mockAuth.Object, mockNotifier.Object, nav, mockJs.Object);

            var result = await service.UpdateEmailAsync("new@example.com", "currentPassword");

            Assert.IsTrue(result.IsFailed);
            Assert.AreEqual("Kunde inte uppdatera e-post.", result.Errors[0].Message);
        }

        [TestMethod]
        public async Task UpdatePasswordAsync_CurrentUserIsNull_ReturnsFailureWithCorrectMessage()
        {
            var mockAuth = new Mock<IAuthClient>();
            var mockNotifier = new Mock<IAuthStateNotifier>();
            var mockJs = new Mock<IJSRuntime>();
            var nav = new TestNavigationManager();

            mockAuth.SetupGet(a => a.CurrentUserEmail).Returns((string?)null);
            var service = new AuthService(mockAuth.Object, mockNotifier.Object, nav, mockJs.Object);

            Result result = await service.UpdatePasswordAsync("currentPassword", "newPassword");

            Assert.IsTrue(result.IsFailed);
            Assert.AreEqual("Kunde inte hämta användaruppgifter.", result.Errors[0].Message);
        }

        [TestMethod]
        public async Task UpdatePasswordAsync_UserEmailIsNull_ReturnsFailureWithCorrectMessage()
        {
            var mockAuth = new Mock<IAuthClient>();
            var mockNotifier = new Mock<IAuthStateNotifier>();
            var mockJs = new Mock<IJSRuntime>();
            var nav = new TestNavigationManager();

            mockAuth.SetupGet(a => a.CurrentUserEmail).Returns((string?)null);

            var service = new AuthService(mockAuth.Object, mockNotifier.Object, nav, mockJs.Object);

            Result result = await service.UpdatePasswordAsync("currentPassword", "newPassword");

            Assert.IsTrue(result.IsFailed);
            Assert.AreEqual("Kunde inte hämta användaruppgifter.", result.Errors[0].Message);
        }

        [TestMethod]
        public async Task UpdatePasswordAsync_EmptyCurrentPassword_ReturnsFailure()
        {
            var mockAuth = new Mock<IAuthClient>();
            var mockNotifier = new Mock<IAuthStateNotifier>();
            var mockJs = new Mock<IJSRuntime>();
            var nav = new TestNavigationManager();

            var service = new AuthService(mockAuth.Object, mockNotifier.Object, nav, mockJs.Object);

            var result = await service.UpdatePasswordAsync("", "newPassword123");

            Assert.IsTrue(result.IsFailed);
            Assert.AreEqual("Nuvarande lösenord måste anges.", result.Errors[0].Message);
            mockAuth.Verify(a => a.SignIn(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdatePasswordAsync_NewPasswordTooShort_ReturnsFailure()
        {
            var mockAuth = new Mock<IAuthClient>();
            var mockNotifier = new Mock<IAuthStateNotifier>();
            var mockJs = new Mock<IJSRuntime>();
            var nav = new TestNavigationManager();

            var service = new AuthService(mockAuth.Object, mockNotifier.Object, nav, mockJs.Object);

            var result = await service.UpdatePasswordAsync("currentPassword", "short");

            Assert.IsTrue(result.IsFailed);
            Assert.AreEqual("Lösenordet måste vara minst 8 tecken.", result.Errors[0].Message);
            mockAuth.Verify(a => a.SignIn(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdatePasswordAsync_NewPasswordMissingUppercase_ReturnsFailure()
        {
            var mockAuth = new Mock<IAuthClient>();
            var mockNotifier = new Mock<IAuthStateNotifier>();
            var mockJs = new Mock<IJSRuntime>();
            var nav = new TestNavigationManager();

            var service = new AuthService(mockAuth.Object, mockNotifier.Object, nav, mockJs.Object);

            var result = await service.UpdatePasswordAsync("currentPassword", "lowercase1");

            Assert.IsTrue(result.IsFailed);
            Assert.AreEqual("Lösenordet måste innehålla minst en stor bokstav.", result.Errors[0].Message);
            mockAuth.Verify(a => a.SignIn(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdatePasswordAsync_NewPasswordMissingLowercase_ReturnsFailure()
        {
            var mockAuth = new Mock<IAuthClient>();
            var mockNotifier = new Mock<IAuthStateNotifier>();
            var mockJs = new Mock<IJSRuntime>();
            var nav = new TestNavigationManager();

            var service = new AuthService(mockAuth.Object, mockNotifier.Object, nav, mockJs.Object);

            var result = await service.UpdatePasswordAsync("currentPassword", "UPPERCASE1");

            Assert.IsTrue(result.IsFailed);
            Assert.AreEqual("Lösenordet måste innehålla minst en liten bokstav.", result.Errors[0].Message);
            mockAuth.Verify(a => a.SignIn(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdatePasswordAsync_ValidPasswordAndUpdate_Succeeds()
        {
            var mockAuth = new Mock<IAuthClient>();
            var mockNotifier = new Mock<IAuthStateNotifier>();
            var mockJs = new Mock<IJSRuntime>();
            var nav = new TestNavigationManager();

            mockAuth.SetupGet(a => a.CurrentUserEmail).Returns("old@example.com");
            mockAuth.Setup(a => a.SignIn("old@example.com", "currentPassword"))
                .ReturnsAsync(new AuthSignInResult { HasUser = true, AccessToken = "token", SessionJson = "{}" });
            mockAuth.Setup(a => a.UpdatePassword("newPassword")).ReturnsAsync(true);

            var service = new AuthService(mockAuth.Object, mockNotifier.Object, nav, mockJs.Object);

            var result = await service.UpdatePasswordAsync("currentPassword", "newPassword");

            Assert.IsTrue(result.IsSuccess);
            mockAuth.Verify(a => a.UpdatePassword("newPassword"), Times.Once);
        }

        [TestMethod]
        public async Task UpdatePasswordAsync_WrongOldPassword_ReturnsFailure()
        {
            var mockAuth = new Mock<IAuthClient>();
            var mockNotifier = new Mock<IAuthStateNotifier>();
            var mockJs = new Mock<IJSRuntime>();
            var nav = new TestNavigationManager();

            mockAuth.SetupGet(a => a.CurrentUserEmail).Returns("old@example.com");
            mockAuth.Setup(a => a.SignIn("old@example.com", "wrongOldPassword"))
                .ThrowsAsync(new Exception("invalid_credentials"));

            var service = new AuthService(mockAuth.Object, mockNotifier.Object, nav, mockJs.Object);

            var result = await service.UpdatePasswordAsync("wrongOldPassword", "newPassword");

            Assert.IsTrue(result.IsFailed);
            Assert.AreEqual("Det gamla lösenordet är felaktigt.", result.Errors[0].Message);
            mockAuth.Verify(a => a.UpdatePassword(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdatePasswordAsync_UpdatePasswordReturnsFalse_ReturnsFailure()
        {
            var mockAuth = new Mock<IAuthClient>();
            var mockNotifier = new Mock<IAuthStateNotifier>();
            var mockJs = new Mock<IJSRuntime>();
            var nav = new TestNavigationManager();

            mockAuth.SetupGet(a => a.CurrentUserEmail).Returns("old@example.com");
            mockAuth.Setup(a => a.SignIn("old@example.com", "currentPassword"))
                .ReturnsAsync(new AuthSignInResult { HasUser = true, AccessToken = "token", SessionJson = "{}" });
            mockAuth.Setup(a => a.UpdatePassword("newPassword")).ReturnsAsync(false);

            var service = new AuthService(mockAuth.Object, mockNotifier.Object, nav, mockJs.Object);

            var result = await service.UpdatePasswordAsync("currentPassword", "newPassword");

            Assert.IsTrue(result.IsFailed);
            Assert.AreEqual("Kunde inte uppdatera lösenord.", result.Errors[0].Message);
        }

        private sealed class TestNavigationManager : NavigationManager
        {
            public string LastNavigatedUri { get; private set; } = string.Empty;

            public TestNavigationManager()
            {
                Initialize("http://localhost/", "http://localhost/");
            }

            protected override void NavigateToCore(string uri, bool forceLoad)
            {
                LastNavigatedUri = ToAbsoluteUri(uri).ToString();
                Uri = LastNavigatedUri;
            }
        }
    }
}
