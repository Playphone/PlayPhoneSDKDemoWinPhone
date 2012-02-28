//
//  MNWSRequestError.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;

namespace PlayPhone.MultiNet.Core.WS
 {
  public class MNWSRequestError
   {
    public const int TRANSPORT_ERROR  = 0;
    public const int SERVER_ERROR     = 1;
    public const int PARSE_ERROR      = 2;
    public const int PARAMETERS_ERROR = 3;

    public int    Domain  { get; set; }
    public string Message { get; set; }

    public MNWSRequestError (int    domain,
                             string message)
     {
      Domain  = domain;
      Message = message;
     }
   }
 }
