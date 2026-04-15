namespace NotificationService.Services;

/// <summary>
/// Provides reusable HTML email templates for notification events.
/// </summary>
public static class EmailTemplates
{
    /// <summary>
    /// Builds the welcome email template for newly registered users.
    /// </summary>
    public static string WelcomeEmail(string fullName) => $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
            <div style='background-color: #1a1a2e; padding: 20px; text-align: center;'>
                <h1 style='color: #e94560; margin: 0;'>ZyntraPay</h1>
                <p style='color: #ffffff; margin: 5px 0;'>Digital Wallet & Rewards</p>
            </div>
            <div style='padding: 30px; background-color: #f9f9f9;'>
                <h2 style='color: #1a1a2e;'>Welcome, {fullName}!</h2>
                <p>Your ZyntraPay account has been created successfully.</p>
                <p>You can now:</p>
                <ul>
                    <li>Add money to your wallet</li>
                    <li>Transfer funds instantly</li>
                    <li>Earn reward points on every transaction</li>
                    <li>Redeem points for exciting rewards</li>
                </ul>
                <div style='text-align: center; margin: 30px 0;'>
                    <a style='background-color: #e94560; color: white; 
                               padding: 12px 30px; text-decoration: none; 
                               border-radius: 5px;'>Get Started</a>
                </div>
            </div>
            <div style='padding: 15px; background-color: #1a1a2e; text-align: center;'>
                <p style='color: #999; font-size: 12px; margin: 0;'>
                    &copy; 2026 ZyntraPay. All rights reserved.
                </p>
            </div>
        </div>";

    /// <summary>
    /// Builds the OTP email template for email verification and reset flows.
    /// </summary>
    public static string OtpEmail(string otp) => $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
            <div style='background-color: #1a1a2e; padding: 20px; text-align: center;'>
                <h1 style='color: #e94560; margin: 0;'>ZyntraPay</h1>
            </div>
            <div style='padding: 30px; background-color: #f9f9f9; text-align: center;'>
                <h2 style='color: #1a1a2e;'>Email Verification</h2>
                <p>Use the OTP below to verify your email address.</p>
                <div style='background-color: #1a1a2e; color: #e94560; 
                            font-size: 36px; font-weight: bold; 
                            padding: 20px; border-radius: 8px; 
                            letter-spacing: 8px; margin: 20px 0;'>
                    {otp}
                </div>
                <p style='color: #666; font-size: 14px;'>
                    This OTP expires in 10 minutes. Do not share it with anyone.
                </p>
            </div>
            <div style='padding: 15px; background-color: #1a1a2e; text-align: center;'>
                <p style='color: #999; font-size: 12px; margin: 0;'>
                    &copy; 2026 ZyntraPay. All rights reserved.
                </p>
            </div>
        </div>";

    /// <summary>
    /// Builds transaction alert email template for wallet credit/debit activity.
    /// </summary>
    public static string TransactionEmail(string type, decimal amount, decimal balance) => $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
            <div style='background-color: #1a1a2e; padding: 20px; text-align: center;'>
                <h1 style='color: #e94560; margin: 0;'>ZyntraPay</h1>
            </div>
            <div style='padding: 30px; background-color: #f9f9f9;'>
                <h2 style='color: #1a1a2e;'>Transaction Alert</h2>
                <table style='width: 100%; border-collapse: collapse;'>
                    <tr style='background-color: #e8f4f8;'>
                        <td style='padding: 12px; border: 1px solid #ddd;'>
                            <strong>Type</strong>
                        </td>
                        <td style='padding: 12px; border: 1px solid #ddd;'>{type}</td>
                    </tr>
                    <tr>
                        <td style='padding: 12px; border: 1px solid #ddd;'>
                            <strong>Amount</strong>
                        </td>
                        <td style='padding: 12px; border: 1px solid #ddd;'>
                            Rs. {amount:F2}
                        </td>
                    </tr>
                    <tr style='background-color: #e8f4f8;'>
                        <td style='padding: 12px; border: 1px solid #ddd;'>
                            <strong>Available Balance</strong>
                        </td>
                        <td style='padding: 12px; border: 1px solid #ddd;'>
                            Rs. {balance:F2}
                        </td>
                    </tr>
                </table>
                <p style='color: #666; font-size: 13px; margin-top: 20px;'>
                    If you did not initiate this transaction, 
                    please contact support immediately.
                </p>
            </div>
            <div style='padding: 15px; background-color: #1a1a2e; text-align: center;'>
                <p style='color: #999; font-size: 12px; margin: 0;'>
                    &copy; 2026 ZyntraPay. All rights reserved.
                </p>
            </div>
        </div>";

    /// <summary>
    /// Builds KYC status email template for approved/rejected outcomes.
    /// </summary>
    public static string KycStatusEmail(string status, string? reason = null) => $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
            <div style='background-color: #1a1a2e; padding: 20px; text-align: center;'>
                <h1 style='color: #e94560; margin: 0;'>ZyntraPay</h1>
            </div>
            <div style='padding: 30px; background-color: #f9f9f9;'>
                <h2 style='color: {(status == "Approved" ? "#27ae60" : "#e74c3c")};'>
                    KYC {status}
                </h2>
                <p>Your KYC verification has been <strong>{status}</strong>.</p>
                {(status == "Rejected" && reason != null
                    ? $"<p><strong>Reason:</strong> {reason}</p>"
                    : "<p>You can now access all ZyntraPay features.</p>")}
            </div>
            <div style='padding: 15px; background-color: #1a1a2e; text-align: center;'>
                <p style='color: #999; font-size: 12px; margin: 0;'>
                    &copy; 2026 ZyntraPay. All rights reserved.
                </p>
            </div>
        </div>";
}