const youtubeUrlPattern = /(?:https?:\/\/)?(?:www\.)?(?:youtube\.com\/(?:[^/\n\s]+\/\S+\/|(?:v|e(?:mbed)?)\/|\S*?[?&]v=)|youtu\.be\/)([a-zA-Z0-9_-]{11})/;

export const isYouTubeVideoLink = (url: string): boolean =>
  youtubeUrlPattern.test(url);
