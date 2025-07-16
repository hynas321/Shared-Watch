export const useRoomHelper = () => {
  const checkIfIsYouTubeVideoLink = (url: string): boolean => {
    const pattern =
      /(?:https?:\/\/)?(?:www\.)?(?:youtube\.com\/(?:[^\/\n\s]+\/\S+\/|(?:v|e(?:mbed)?)\/|\S*?[?&]v=)|youtu\.be\/)([a-zA-Z0-9_-]{11})/;
    const regex = new RegExp(pattern);
    const match = url.match(regex);

    return !!match;
  };

  return { checkIfIsYouTubeVideoLink }
}