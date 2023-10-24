export default function VideoPlayer() {
  return (
    <div className="rounded-top-5 bg-dark pt-4 px-5">
      <div className="embed-responsive embed-responsive-16by9">
      <iframe
          className="embed-responsive-item"
          src="https://www.youtube.com/embed/zpOULjyy-n8?rel=0"
          title="YouTube Video"
          allowFullScreen
          style={{
            height: "500px",
            width: "100%"
          }}
        ></iframe>
      </div>
    </div>
  )
}
