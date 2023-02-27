using System.Runtime.InteropServices;
using Braintree;

namespace ShopProject_Utility.BrainTree;

public interface IBrainTreeGate
{
    IBraintreeGateway CreateGateway();
    IBraintreeGateway GetGateway();
}