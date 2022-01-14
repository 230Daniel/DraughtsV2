import React from "react";

import styles from "./Piece.Module.css";

export default class Piece extends React.Component {
	render() {
		// If the piece passed is -1 (empty), we don't need to render anything
		if (this.props.piece === -1)
			return null;

		// Render the piece, and if it's a king (value is 2 or 3) add a king div for the crown.
		return (
			<div
				className={styles.container}
				style={{
					// If this piece is animating we set the transform and opacity CSS properties depending on the values passed from the Board.
					// These changes are made smoothly thanks to a CSS transition property, resulting in a clean animation.
					transform: `translate(${this.props.transformX * 100}%, ${this.props.transformY * 100}%) scale(${this.props.taken ? 0 : 1})`,
					opacity: this.props.taken ? 0 : 1,
					zIndex: this.props.transformX === 0 ? 100 : 200
				}}>
				<div className={this.getClassNames()}>
					<div className={styles.inside}>
						{this.props.piece >= 2 &&
							<div className={styles.king} />
						}
					</div>
				</div>
			</div>
		);
	}

	// Returns a className based on this component's props
	// Eg. if "selected=true" is passed then the selected class is added
	getClassNames() {
		var classNames = [styles.piece, this.props.piece % 2 === 0 ? styles.black : styles.white];
		if (this.props.selected) classNames.push(styles.selected);
		return classNames.join(" ");
	}
}
