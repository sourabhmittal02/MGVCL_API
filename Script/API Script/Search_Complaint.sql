
create PROCEDURE [dbo].[Search_Complaint]
	-- Add the parameters for the stored procedure here
	@kno bigint
AS
BEGIN
	SELECT a.*,b.COMPLAINT_status cst from COMPLAINT(NOLOCK) a, 
	MST_COMPLAINT_STEPS(NOLOCK) b where a.KNO=@kno and a.COMPLAINT_status=b.ID 
END