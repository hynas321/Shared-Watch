export type UserPermissions = {
  maxUsers: number,
  isSendingChatMessagesAllowed: boolean,
  isAddingVideosAllowed: boolean,
  isRemovingVideosAllowed: boolean,
  isPlayingVideosOutsideOfPlaylistAllowed: boolean,
  isStartingPausingVideosAllowed: boolean,
  isSkippingVideosAllowed: boolean
}