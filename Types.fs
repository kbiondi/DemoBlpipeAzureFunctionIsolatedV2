namespace Bloomberglp.Blpapi.Examples

module Schemas =

  type ResponseError =
    {
      Category: string option
      Message: string option
    }

  type FieldDatum =
    {
      Name: string option
      Value: string option
    }

  type ErrorInfo =
    {
      Category: string option
      Message: string option
    }    

  type FieldDataException =
    {
      FieldId: string option
      Message: string option
      ErrorInfo: ErrorInfo option
    }

  type SecurityError =
    {
      Category: string option
      Message: string option
    }        

  type SecurityData = 
    {
      Security: string option
      FieldData: FieldDatum list option
      FieldDataExceptions: FieldDataException list option
      SecurityError: SecurityError option
    }    

  type ReferenceDataResponse =
    {
      RequestId: string option
      ResponseError: ResponseError option
      SecurityData: SecurityData list option
    }